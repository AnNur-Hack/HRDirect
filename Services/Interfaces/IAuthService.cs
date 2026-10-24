using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;

namespace HRDirectAPI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse> LoginAsync(LoginRequest request);
    Task<ApiResponse> SetupAccountAsync(SetupAccountRequest request);
    
    Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request);

    Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request);
    
    Task<ApiResponse> ChangePasswordAsync(int employeeId, ChangePasswordRequest request);

}