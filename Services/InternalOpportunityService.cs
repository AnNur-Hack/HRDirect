using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRDirectAPI.Services;

public class InternalOpportunityService(ApplicationDbContext context, IEmailService emailService) : IInternalOpportunityService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IEmailService _emailService = emailService;
    
     public async Task<ApiResponse> CreateOpportunityAsync(
        CreateOpportunityRequest request,
        int hrEmployeeId)
    {
        if (request.ClosingAtUtc <= DateTime.UtcNow)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The closing date must be in the future.");
        }

        var opportunity = new InternalOpportunity
        {
            Title = request.Title,
            Type = request.Type,
            Department = request.Department,
            Description = request.Description,
            EligibilityText = request.EligibilityText,
            AllowedDepartments = request.AllowedDepartments,
            AllowedGrades = request.AllowedGrades,
            ClosingAtUtc = request.ClosingAtUtc,
            Status = OpportunityStatus.Draft,
            CreatedByEmployeeId = hrEmployeeId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.InternalOpportunities.Add(opportunity);

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            opportunity,
            "Opportunity draft created successfully.");
    }

    public async Task<ApiResponse> UpdateOpportunityAsync(
        int opportunityId,
        UpdateOpportunityRequest request)
    {
        var opportunity = await _context.InternalOpportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId);

        if (opportunity == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Opportunity not found.");
        }

        if (opportunity.Status == OpportunityStatus.Closed)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Closed opportunities cannot be edited.");
        }

        opportunity.Title = request.Title;
        opportunity.Type = request.Type;
        opportunity.Department = request.Department;
        opportunity.Description = request.Description;
        opportunity.EligibilityText = request.EligibilityText;
        opportunity.AllowedDepartments = request.AllowedDepartments;
        opportunity.AllowedGrades = request.AllowedGrades;
        opportunity.ClosingAtUtc = request.ClosingAtUtc;

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            opportunity,
            "Opportunity updated successfully.");
    }

    public async Task<ApiResponse> PublishOpportunityAsync(
        int opportunityId)
    {
        var opportunity = await _context.InternalOpportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId);

        if (opportunity == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Opportunity not found.");
        }

        if (opportunity.Status == OpportunityStatus.Closed)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Closed opportunities cannot be reopened.");
        }

        if (opportunity.ClosingAtUtc <= DateTime.UtcNow)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "The closing date must be in the future.");
        }

        opportunity.Status = OpportunityStatus.Published;
        opportunity.PublishedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            opportunity,
            "Opportunity published successfully.");
    }

    public async Task<ApiResponse> CloseOpportunityAsync(
        int opportunityId)
    {
        var opportunity = await _context.InternalOpportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId);

        if (opportunity == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Opportunity not found.");
        }

        opportunity.Status = OpportunityStatus.Closed;

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            opportunity,
            "Opportunity closed successfully.");
    }

    public async Task<ApiResponse> GetOpportunitiesAsync(bool isHr)
    {
        var query = _context.InternalOpportunities.AsQueryable();

        if (!isHr)
        {
            query = query.Where(o =>
                o.Status == OpportunityStatus.Published &&
                o.ClosingAtUtc > DateTime.UtcNow);
        }

        var opportunities = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            opportunities,
            "Opportunities retrieved successfully.");
    }

    public async Task<ApiResponse> ApplyForOpportunityAsync(
        int opportunityId,
        ApplyForOpportunityRequest request,
        int employeeId,
        string cvFilePath,
        string? cvFileName)
    {
        var opportunity = await _context.InternalOpportunities
        .FirstOrDefaultAsync(o => o.Id == opportunityId);

    if (opportunity == null)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "Opportunity not found.");
    }

    if (opportunity.Status != OpportunityStatus.Published)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "This opportunity is not published.");
    }

    if (opportunity.ClosingAtUtc <= DateTime.UtcNow)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "The application deadline has passed.");
    }

    var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.Id == employeeId);

    if (employee == null)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "Employee not found.");
    }

    if (!employee.IsActive)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "Your employee account is inactive.");
    }

    var departmentAllowed =
        opportunity.AllowedDepartments.Count == 0 ||
        opportunity.AllowedDepartments.Any(
            department => string.Equals(
                department,
                employee.Department,
                StringComparison.OrdinalIgnoreCase));

    var gradeAllowed =
        opportunity.AllowedGrades.Count == 0 ||
        opportunity.AllowedGrades.Any(
            grade => string.Equals(
                grade,
                employee.Grade,
                StringComparison.OrdinalIgnoreCase));

    if (!departmentAllowed || !gradeAllowed)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "You are not eligible to apply for this opportunity.");
    }

    var existingApplication = await _context.OpportunityApplications
        .AnyAsync(a =>
            a.InternalOpportunityId == opportunityId &&
            a.EmployeeId == employeeId);

    if (existingApplication)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "You have already applied for this opportunity.");
    }

    var application = new OpportunityApplication
    {
        InternalOpportunityId = opportunityId,
        EmployeeId = employeeId,
        SupportingStatement = request.SupportingStatement,
        CvFilePath = cvFilePath,
        CvFileName = cvFileName,
        DepartmentAtApplication = employee.Department,
        GradeAtApplication = employee.Grade,
        AppliedAtUtc = DateTime.UtcNow,
        Status = OpportunityApplicationStatus.Submitted
    };

    _context.OpportunityApplications.Add(application);

    await _context.SaveChangesAsync();

    return ApiResponse.ResponseHelper.SuccessResponse(
        new
        {
            application.Id,
            application.InternalOpportunityId,
            application.Status,
            application.AppliedAtUtc
        },
        "Application submitted successfully.");
    }

    public async Task<ApiResponse> GetMyApplicationsAsync(int employeeId)
    {
        var applications = await _context.OpportunityApplications
            .Where(a => a.EmployeeId == employeeId)
            .Include(a => a.InternalOpportunity)
            .OrderByDescending(a => a.AppliedAtUtc)
            .Select(a => new
            {
                a.Id,
                a.InternalOpportunityId,
                OpportunityTitle = a.InternalOpportunity.Title,
                OpportunityType = a.InternalOpportunity.Type,
                a.SupportingStatement,
                a.CvFileName,
                a.Status,
                a.DepartmentAtApplication,
                a.GradeAtApplication,
                a.AppliedAtUtc,
                a.WithdrawnAtUtc
            })
            .ToListAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            applications,
            "Applications retrieved successfully.");
    }

    public async Task<ApiResponse> GetApplicantsAsync(int opportunityId)
    {
        var opportunityExists = await _context.InternalOpportunities
            .AnyAsync(o => o.Id == opportunityId);

        if (!opportunityExists)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Opportunity not found.");
        }

        var applicants = await (
                from application in _context.OpportunityApplications
                join employee in _context.Employees
                    on application.EmployeeId equals employee.Id
                where application.InternalOpportunityId == opportunityId
                orderby application.AppliedAtUtc descending
                select new
                {
                    application.Id,
                    application.EmployeeId,
                    employee.EmployeeNumber,
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.Department,
                    employee.Grade,
                    application.SupportingStatement,
                    application.CvFileName,
                    application.Status,
                    application.DepartmentAtApplication,
                    application.GradeAtApplication,
                    application.AppliedAtUtc
                })
            .ToListAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            applicants,
            "Applicants retrieved successfully.");
    }

    public async Task<ApiResponse> UpdateApplicationStatusAsync(
        int applicationId,
        UpdateApplicationStatusRequest request,
        int hrEmployeeId)
    {
        var application = await _context.OpportunityApplications
        .FirstOrDefaultAsync(a => a.Id == applicationId);

    if (application == null)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "Application not found.");
    }

    var currentStatus = application.Status;
    var newStatus = request.Status;

    var validTransition =
        (currentStatus == OpportunityApplicationStatus.Submitted &&
         (newStatus == OpportunityApplicationStatus.Shortlisted ||
          newStatus == OpportunityApplicationStatus.Rejected))
        ||
        (currentStatus == OpportunityApplicationStatus.Shortlisted &&
         (newStatus == OpportunityApplicationStatus.Selected ||
          newStatus == OpportunityApplicationStatus.Rejected));

    if (!validTransition)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "Invalid status transition.");
    }

    var employee = await _context.Employees
        .FirstOrDefaultAsync(e => e.Id == application.EmployeeId);

    if (employee == null)
    {
        return ApiResponse.ResponseHelper.FailureResponse(
            "Applicant not found.");
    }

    application.Status = newStatus;

    var history = new OpportunityApplicationHistory
    {
        OpportunityApplicationId = application.Id,
        PreviousStatus = currentStatus.ToString(),
        NewStatus = newStatus.ToString(),
        ChangedByEmployeeId = hrEmployeeId,
        ChangedAtUtc = DateTime.UtcNow
    };

    _context.OpportunityApplicationHistories.Add(history);

    await _context.SaveChangesAsync();

    try
    {
        await _emailService.SendEmailAsync(
            employee.Email,
            "Internal Opportunity Application Update",
            $"Hello {employee.FirstName}, your application status has changed " +
            $"from {currentStatus} to {newStatus}.");
    }
    catch
    {
        // Keep the saved status even if the email cannot be sent.
        // We can improve email delivery when configuring FluentEmail.
    }

    return ApiResponse.ResponseHelper.SuccessResponse(
        new
        {
            application.Id,
            PreviousStatus = currentStatus,
            CurrentStatus = application.Status
        },
        "Application status updated successfully.");
    }

    public async Task<ApiResponse> WithdrawApplicationAsync(
        int applicationId,
        int employeeId)
    {
        var application = await _context.OpportunityApplications
            .FirstOrDefaultAsync(a =>
                a.Id == applicationId &&
                a.EmployeeId == employeeId);

        if (application == null)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "Application not found.");
        }

        if (application.Status != OpportunityApplicationStatus.Submitted &&
            application.Status != OpportunityApplicationStatus.Shortlisted)
        {
            return ApiResponse.ResponseHelper.FailureResponse(
                "This application can no longer be withdrawn.");
        }

        var previousStatus = application.Status;

        application.Status = OpportunityApplicationStatus.Withdrawn;
        application.WithdrawnAtUtc = DateTime.UtcNow;

        var history = new OpportunityApplicationHistory
        {
            OpportunityApplicationId = application.Id,
            PreviousStatus = previousStatus.ToString(),
            NewStatus = OpportunityApplicationStatus.Withdrawn.ToString(),
            ChangedByEmployeeId = employeeId,
            ChangedAtUtc = DateTime.UtcNow
        };

        _context.OpportunityApplicationHistories.Add(history);

        await _context.SaveChangesAsync();

        return ApiResponse.ResponseHelper.SuccessResponse(
            new
            {
                application.Id,
                application.Status,
                application.WithdrawnAtUtc
            },
            "Application withdrawn successfully.");
    }
}