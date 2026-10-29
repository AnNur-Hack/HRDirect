using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRDirectAPI.Services.Implementations;

public class PayslipService(ApplicationDbContext context, IEmailService emailService) : IPayslipService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IEmailService _emailService = emailService;

    public async Task<ApiResponse> CreatePayslipAsync(
        CreatePayslipRequest request,
        int financeEmployeeId)
    {
        if (request.EmployeeId == financeEmployeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Finance users cannot prepare their own payslips.");
        }

        if (request.Month < 1 || request.Month > 12 ||
            request.Year < 1)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A valid month and year are required.");
        }

        if (request.BasicSalary < 0 ||
            request.Items.Any(item => item.Amount < 0))
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Salary, allowances and deductions cannot be negative.");
        }

        var employeeExists = await _context.Employees
            .AnyAsync(e => e.Id == request.EmployeeId);

        if (!employeeExists)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Employee not found.");
        }

        var duplicate = await _context.Payslips.AnyAsync(p =>
            p.EmployeeId == request.EmployeeId &&
            p.Month == request.Month &&
            p.Year == request.Year &&
            p.ParentPayslipId == null);

        if (duplicate)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A payslip already exists for this employee and period.");
        }

        var totalAllowances = request.Items
            .Where(item => item.Type == PayslipItemType.Allowance)
            .Sum(item => item.Amount);

        var totalDeductions = request.Items
            .Where(item => item.Type == PayslipItemType.Deduction)
            .Sum(item => item.Amount);

        var grossPay = request.BasicSalary + totalAllowances;

        if (totalDeductions > grossPay)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Total deductions cannot exceed gross pay.");
        }

        var payslip = new Payslip
        {
            PayslipReference = $"PAY-{Guid.NewGuid():N}",
            EmployeeId = request.EmployeeId,
            PreparedByEmployeeId = financeEmployeeId,
            Month = request.Month,
            Year = request.Year,
            Version = 1,
            BasicSalary = request.BasicSalary,
            GrossPay = grossPay,
            NetPay = grossPay - totalDeductions,
            Status = PayslipStatus.Draft,
            IsCurrent = false,
            CreatedAtUtc = DateTime.UtcNow,
            Items = request.Items.Select(item => new PayslipItem
            {
                Name = item.Name,
                Type = item.Type,
                Amount = item.Amount
            }).ToList()
        };

        _context.Payslips.Add(payslip);

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            payslip,
            "Payslip draft created successfully.");
    }

    public async Task<ApiResponse> UpdatePayslipAsync(
        int payslipId,
        UpdatePayslipRequest request,
        int financeEmployeeId)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == payslipId);

        if (payslip == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Payslip not found.");
        }

        if (payslip.PreparedByEmployeeId != financeEmployeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "You cannot edit a payslip prepared by another Finance user.");
        }

        if (payslip.Status != PayslipStatus.Draft)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Only draft payslips can be edited.");
        }

        if (request.BasicSalary < 0 ||
            request.Items.Any(item => item.Amount < 0))
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Salary, allowances and deductions cannot be negative.");
        }

        var totalAllowances = request.Items
            .Where(item => item.Type == PayslipItemType.Allowance)
            .Sum(item => item.Amount);

        var totalDeductions = request.Items
            .Where(item => item.Type == PayslipItemType.Deduction)
            .Sum(item => item.Amount);

        var grossPay = request.BasicSalary + totalAllowances;

        if (totalDeductions > grossPay)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Total deductions cannot exceed gross pay.");
        }

        payslip.BasicSalary = request.BasicSalary;
        payslip.GrossPay = grossPay;
        payslip.NetPay = grossPay - totalDeductions;

        _context.PayslipItems.RemoveRange(payslip.Items);

        payslip.Items = request.Items.Select(item => new PayslipItem
        {
            PayslipId = payslip.Id,
            Name = item.Name,
            Type = item.Type,
            Amount = item.Amount
        }).ToList();

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            payslip,
            "Payslip draft updated successfully.");
    }

    public async Task<ApiResponse> PublishPayslipAsync(
        int payslipId,
        int financeEmployeeId)
    {
        var payslip = await _context.Payslips
            .FirstOrDefaultAsync(p => p.Id == payslipId);

        if (payslip == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Payslip not found.");
        }

        if (payslip.PreparedByEmployeeId != financeEmployeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "You cannot publish a payslip prepared by another Finance user.");
        }

        if (payslip.EmployeeId == financeEmployeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Finance users cannot publish their own payslips.");
        }

        if (payslip.Status != PayslipStatus.Draft)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Only draft payslips can be published.");
        }

        if (payslip.ParentPayslipId.HasValue)
        {
            var previousVersion = await _context.Payslips
                .FirstOrDefaultAsync(p =>
                    p.EmployeeId == payslip.EmployeeId &&
                    p.Month == payslip.Month &&
                    p.Year == payslip.Year &&
                    p.IsCurrent &&
                    p.Status == PayslipStatus.Published);

            if (previousVersion == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "The current published version could not be found.");
            }

            previousVersion.Status = PayslipStatus.Superseded;
            previousVersion.IsCurrent = false;
        }

        payslip.Status = PayslipStatus.Published;
        payslip.IsCurrent = true;
        payslip.PublishedAtUtc = DateTime.UtcNow;

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == payslip.EmployeeId);

        if (employee != null)
        {
            _context.EmailQueues.Add(new EmailQueue
            {
                RecipientEmail = employee.Email,
                Subject = "Your Payslip Is Available",
                Body = $"Your payslip for {payslip.Month}/{payslip.Year}, version {payslip.Version}, is now available.",
                IsSent = false,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                employee.Email,
                "Your Payslip Is Available",
                $"Your payslip for {payslip.Month}/{payslip.Year}, version {payslip.Version}, is now available.");
        }
        else
        {
            await _context.SaveChangesAsync();
        }
        return ApiResponse.ResponseHelper.SuccessResponse(
            payslip,
            "Payslip published successfully.");
    }


public async Task<ApiResponse> CreateCorrectionAsync(
        int payslipId,
        CorrectPayslipRequest request,
        int financeEmployeeId)
    {
        var original = await _context.Payslips
            .FirstOrDefaultAsync(p => p.Id == payslipId);

        if (original == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Payslip not found.");
        }

        if (original.EmployeeId == financeEmployeeId)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Finance users cannot prepare their own payslips.");
        }

        if (original.Status != PayslipStatus.Published ||
            !original.IsCurrent)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Corrections can only be created for the current published payslip.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A correction reason is required.");
        }

        var originalPayslipId =
            original.ParentPayslipId ?? original.Id;

        var pendingRevision = await _context.Payslips.AnyAsync(p =>
            p.ParentPayslipId == originalPayslipId &&
            p.Status == PayslipStatus.Draft);

        if (pendingRevision)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "A draft correction already exists for this payslip.");
        }

        if (request.BasicSalary < 0 ||
            request.Items.Any(item => item.Amount < 0))
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Salary, allowances and deductions cannot be negative.");
        }

        var totalAllowances = request.Items
            .Where(item => item.Type == PayslipItemType.Allowance)
            .Sum(item => item.Amount);

        var totalDeductions = request.Items
            .Where(item => item.Type == PayslipItemType.Deduction)
            .Sum(item => item.Amount);

        var grossPay = request.BasicSalary + totalAllowances;

        if (totalDeductions > grossPay)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Total deductions cannot exceed gross pay.");
        }

        var highestVersion = await _context.Payslips
            .Where(p =>
                p.EmployeeId == original.EmployeeId &&
                p.Month == original.Month &&
                p.Year == original.Year &&
                (p.Id == originalPayslipId ||
                 p.ParentPayslipId == originalPayslipId))
            .MaxAsync(p => (int?)p.Version) ?? original.Version;

        var revision = new Payslip
        {
            PayslipReference = original.PayslipReference,
            EmployeeId = original.EmployeeId,
            PreparedByEmployeeId = financeEmployeeId,
            Month = original.Month,
            Year = original.Year,
            Version = highestVersion + 1,
            ParentPayslipId = originalPayslipId,
            CorrectionReason = request.Reason,
            BasicSalary = request.BasicSalary,
            GrossPay = grossPay,
            NetPay = grossPay - totalDeductions,
            Status = PayslipStatus.Draft,
            IsCurrent = false,
            CreatedAtUtc = DateTime.UtcNow,
            Items = request.Items.Select(item => new PayslipItem
            {
                Name = item.Name,
                Type = item.Type,
                Amount = item.Amount
            }).ToList()
        };

        _context.Payslips.Add(revision);

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            revision,
            "Payslip correction draft created successfully.");
    }

    public async Task<ApiResponse> GetMyPayslipsAsync(
        int employeeId,
        int? month,
        int? year)
    {
        var query = _context.Payslips
            .Where(p =>
                p.EmployeeId == employeeId &&
                p.Status == PayslipStatus.Published &&
                p.IsCurrent);

        if (month.HasValue)
        {
            query = query.Where(p => p.Month == month.Value);
        }

        if (year.HasValue)
        {
            query = query.Where(p => p.Year == year.Value);
        }

        var payslips = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            payslips,
            "Published payslips retrieved successfully.");
    }

    public async Task<(
        byte[]? FileBytes,
        string? FileName,
        string? ErrorMessage)> GetPayslipPdfAsync(
        int payslipId,
        int requesterEmployeeId,
        bool isFinance)
    {
        var payslip = await _context.Payslips
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == payslipId);

        if (payslip == null)
        {
            return (null, null, "Payslip not found.");
        }

        if (payslip.Status != PayslipStatus.Published &&
            payslip.Status != PayslipStatus.Superseded)
        {
            return (null, null, "This payslip has not been published.");
        }

        if (payslip.EmployeeId != requesterEmployeeId && !isFinance)
        {
            return (null, null, "You are not allowed to download this payslip.");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == payslip.EmployeeId);

        if (employee == null)
        {
            return (null, null, "Employee not found.");
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var pdfBytes = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(style => style.FontSize(11));

                page.Header()
                    .Text("EMPLOYEE PAYSLIP")
                    .Bold()
                    .FontSize(22)
                    .AlignCenter();

                page.Content().Column(column =>
                {
                    column.Spacing(12);

                    column.Item().PaddingTop(20).Text(
                        $"Employee Name: {employee.FirstName} {employee.LastName}");

                    column.Item().Text(
                        $"Employee Number: {employee.EmployeeNumber}");

                    column.Item().Text(
                        $"Pay Period: {new DateTime(payslip.Year, payslip.Month, 1):MMMM yyyy}");

                    column.Item().Text(
                        $"Payslip Version: {payslip.Version}");

                    column.Item().PaddingTop(15).Text("EARNINGS")
                        .Bold().FontSize(14);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle)
                                .Text("Description").Bold();

                            header.Cell().Element(CellStyle)
                                .AlignRight().Text("Amount (NGN)").Bold();
                        });

                        table.Cell().Element(CellStyle).Text("Basic Salary");
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(payslip.BasicSalary.ToString("N2"));

                        foreach (var item in payslip.Items.Where(item => item.Type == PayslipItemType.Allowance))
                        {
                            table.Cell().Element(CellStyle).Text(item.Name);
                            table.Cell().Element(CellStyle).AlignRight()
                                .Text(item.Amount.ToString("N2"));
                        }

                        table.Cell().Element(CellStyle).Text("Gross Pay").Bold();
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(payslip.GrossPay.ToString("N2")).Bold();
                    });

                    column.Item().PaddingTop(15).Text("DEDUCTIONS")
                        .Bold().FontSize(14);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle)
                                .Text("Description").Bold();

                            header.Cell().Element(CellStyle)
                                .AlignRight().Text("Amount (NGN)").Bold();
                        });

                        foreach (var item in payslip.Items.Where(item => item.Type == PayslipItemType.Deduction))
                        {
                            table.Cell().Element(CellStyle).Text(item.Name);
                            table.Cell().Element(CellStyle).AlignRight()
                                .Text(item.Amount.ToString("N2"));
                        }

                        table.Cell().Element(CellStyle).Text("Net Pay").Bold();
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(payslip.NetPay.ToString("N2")).Bold();
                    });

                    column.Item().PaddingTop(20)
                        .Text("This document is an official employee payslip.")
                        .Italic()
                        .FontSize(9);
                });

                page.Footer()
                    .AlignCenter()
                    .Text($"Generated on {DateTime.UtcNow:dd MMM yyyy} | HRDirect");
            });
        }).GeneratePdf();

        var fileName =
            $"Payslip_{payslip.Year}_{payslip.Month:D2}_V{payslip.Version}.pdf";

        return (pdfBytes, fileName, null);
    }

    static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(6)
            .PaddingHorizontal(4);
    }
}