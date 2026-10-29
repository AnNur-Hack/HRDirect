using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;

namespace HRDirectAPI.Services.Interfaces;

public interface IInternalOpportunityService
{
    Task<ApiResponse> CreateOpportunityAsync(
        CreateOpportunityRequest request,
        int hrEmployeeId);

    Task<ApiResponse> UpdateOpportunityAsync(
        int opportunityId,
        UpdateOpportunityRequest request);

    Task<ApiResponse> PublishOpportunityAsync(
        int opportunityId);

    Task<ApiResponse> CloseOpportunityAsync(
        int opportunityId);

    Task<ApiResponse> GetOpportunitiesAsync(
        bool isHr);

    Task<ApiResponse> ApplyForOpportunityAsync(
        int opportunityId,
        ApplyForOpportunityRequest request,
        int employeeId,
        string cvFilePath,
        string? cvFileName);

    Task<ApiResponse> GetMyApplicationsAsync(
        int employeeId);

    Task<ApiResponse> GetApplicantsAsync(
        int opportunityId);

    Task<ApiResponse> UpdateApplicationStatusAsync(
        int applicationId,
        UpdateApplicationStatusRequest request,
        int hrEmployeeId);

    Task<ApiResponse> WithdrawApplicationAsync(
        int applicationId,
        int employeeId);
}