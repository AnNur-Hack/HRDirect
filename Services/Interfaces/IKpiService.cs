using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;

namespace HRDirectAPI.Services.Interfaces;

public interface IKpiService
{
    Task<ApiResponse> CreateKpiPlanAsync(
        CreateKpiPlanRequest request,
        int hrEmployeeId);

    Task<ApiResponse> AddKpiItemAsync(
        int planId,
        CreateKpiItemRequest request);

    Task<ApiResponse> ActivateKpiPlanAsync(
        int planId,
        ActivateKpiPlanRequest request);

    Task<ApiResponse> GetMyKpiPlansAsync(
        int employeeId);

    Task<ApiResponse> UpdateKpiProgressAsync(
        int planId,
        UpdateKpiProgressRequest request,
        int employeeId);

    Task<ApiResponse> SubmitKpiPlanAsync(
        int planId,
        int employeeId);

    Task<ApiResponse> ReviewKpiPlanAsync(
        int planId,
        ReviewKpiPlanRequest request,
        int reviewerEmployeeId);

    Task<ApiResponse> ReopenKpiPlanAsync(
        int planId,
        ReopenKpiPlanRequest request,
        int hrEmployeeId);
}