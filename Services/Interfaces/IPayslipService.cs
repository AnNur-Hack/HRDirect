using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;

namespace HRDirectAPI.Services.Interfaces;

public interface IPayslipService
{
    Task<ApiResponse> CreatePayslipAsync(
        CreatePayslipRequest request,
        int financeEmployeeId);

    Task<ApiResponse> UpdatePayslipAsync(
        int payslipId,
        UpdatePayslipRequest request,
        int financeEmployeeId);

    Task<ApiResponse> PublishPayslipAsync(
        int payslipId,
        int financeEmployeeId);

    Task<ApiResponse> CreateCorrectionAsync(
        int payslipId,
        CorrectPayslipRequest request,
        int financeEmployeeId);

    Task<ApiResponse> GetMyPayslipsAsync(
        int employeeId,
        int? month,
        int? year);

    Task<(byte[]? FileBytes, string? FileName, string? ErrorMessage)>
        GetPayslipPdfAsync(
            int payslipId,
            int requesterEmployeeId,
            bool isFinance);
}