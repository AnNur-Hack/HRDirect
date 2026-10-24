using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;

namespace HRDirectAPI.Services.Interfaces;

public interface ITrainingService
{
    
    Task<ApiResponse> CreateDraftAsync(
        int employeeId,
        CreateTrainingRequest request);

    Task<ApiResponse> GetMyRequestsAsync(int employeeId);

    Task<ApiResponse> UpdateDraftAsync(
        int employeeId,
        int requestId,
        UpdateTrainingRequest request);

    Task<ApiResponse> SubmitForApprovalAsync(
        int employeeId,
        int requestId);
    
    Task<ApiResponse> ApproveByManagerAsync(
        int managerId,
        int requestId,
        ManagerTrainingDecisionRequest request);

    Task<ApiResponse> RejectByManagerAsync(
        int managerId,
        int requestId,
        ManagerTrainingDecisionRequest request);
    
    Task<ApiResponse> ApproveByHRAsync(
        int hrEmployeeId,
        int requestId,
        HRTrainingDecisionRequest request);

    Task<ApiResponse> ReturnToManagerAsync(
        int hrEmployeeId,
        int requestId,
        HRTrainingDecisionRequest request);
    
    Task<ApiResponse> ApproveByFinanceAsync(
        int financeEmployeeId,
        int requestId,
        FinanceTrainingDecisionRequest request);

    Task<ApiResponse> RejectByFinanceAsync(
        int financeEmployeeId,
        int requestId,
        FinanceTrainingDecisionRequest request);

    public Task<ApiResponse> ReturnCompletionEvidenceAsync(
        int hrEmployeeId,
        int requestId,
        TrainingCompletionDecisionRequest request);

    public Task<ApiResponse> CompleteTrainingAsync(
        int hrEmployeeId,
        int requestId,
        TrainingCompletionDecisionRequest request);

    public Task<ApiResponse> CloseWithoutEvidenceAsync(
        int hrEmployeeId,
        int requestId,
        TrainingCompletionDecisionRequest request);

    public Task<ApiResponse> SubmitCompletionEvidenceAsync(
        int employeeId,
        int requestId,
        TrainingCompletionEvidenceRequest request);
    
    Task<ApiResponse> DownloadCompletionEvidenceAsync(
        int employeeId,
        int requestId);
    
    Task<ApiResponse> WithdrawTrainingRequestAsync(
        int employeeId,
        int requestId,
        WithdrawTrainingRequest request);

    Task<ApiResponse> ReassignTrainingReviewerAsync(
        int adminEmployeeId,
        int requestId,
        ReassignTrainingReviewerRequest request);

    Task<ApiResponse> GetPendingManagerRequestsAsync(
        int adminEmployeeId);


}