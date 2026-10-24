using System.Security.Cryptography;
using System.Text;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Helpers;
using HRDirectAPI.Repositories.Interfaces;
using HRDirectAPI.Services.Interfaces;

namespace HRDirectAPI.Services.Implementations;

public class AuthService
    (IAuthRepository authRepository, IAuditLogRepository auditLogRepository, JwtHelper jwtHelper,
    ILogger<AuthService> logger, IEmailService emailService) : IAuthService
{
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IAuditLogRepository _auditLogRepository = auditLogRepository;
    private readonly JwtHelper _jwtHelper = jwtHelper;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailService _emailService = emailService;
    
    
    public async Task<ApiResponse> LoginAsync(LoginRequest request)
    {
       
        try
        {
            var employee = await _authRepository
                .GetEmployeeByEmailAsync(request.Email);

            if (employee == null || !employee.IsActive)
            {
                _logger.LogWarning(
                    "Login failed: account not found or inactive.");

                await LogAuditAsync(
                    null,
                    "FailedLogin",
                    "Invalid login attempt.");

                return ApiResponse.ResponseHelper.FailureResponse(
                    "Invalid email or password.");
            }

            var now = DateTime.UtcNow;

            if (employee.LockoutEndUtc.HasValue &&
                employee.LockoutEndUtc.Value > now)
            {
                _logger.LogWarning(
                    "Login blocked because the account is locked. EmployeeId: {EmployeeId}",
                    employee.Id);

                await LogAuditAsync(
                    employee.Id,
                    "LoginBlocked",
                    "Login attempted while account was locked.");

                return ApiResponse.ResponseHelper.FailureResponse(
                    "Account is temporarily locked. Please try again later.");
            }

            if (!PasswordHasher.VerifyHashedPassword(
                    request.Password,
                    employee.PasswordHash))
            {
                if (employee.FailedLoginWindowStartUtc == null ||
                    employee.FailedLoginWindowStartUtc.Value.AddMinutes(15) <= now)
                {
                    employee.FailedLoginWindowStartUtc = now;
                    employee.FailedLoginAttempts = 0;
                }

                employee.FailedLoginAttempts++;

                if (employee.FailedLoginAttempts >= 5)
                {
                    employee.LockoutEndUtc = now.AddMinutes(15);

                    _logger.LogWarning(
                        "Account locked after repeated failed login attempts. EmployeeId: {EmployeeId}",
                        employee.Id);
                }

                await _authRepository.UpdateEmployeeAsync(employee);
                await _authRepository.SaveChangesAsync();

                await LogAuditAsync(
                    employee.Id,
                    "FailedLogin",
                    "Incorrect password entered.");

                return ApiResponse.ResponseHelper.FailureResponse(
                    "Invalid email or password.");
            }

            employee.FailedLoginAttempts = 0;
            employee.FailedLoginWindowStartUtc = null;
            employee.LockoutEndUtc = null;
            employee.LastLoginAtUtc = now;

            await _authRepository.UpdateEmployeeAsync(employee);
            await _authRepository.SaveChangesAsync();

            await LogAuditAsync(
                employee.Id,
                "Login",
                "Employee logged in successfully.");

            _logger.LogInformation(
                "Employee logged in successfully. EmployeeId: {EmployeeId}",
                employee.Id);

            var loginResponse = _jwtHelper.GenerateJwt(employee);

            return ApiResponse.ResponseHelper.SuccessResponse(
                loginResponse,
                "Login successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during login.");

            return ApiResponse.ResponseHelper.FailureResponse(
                "An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ApiResponse> SetupAccountAsync(SetupAccountRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Setup token is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Password is required.");
            }

            if (request.Password != request.ConfirmPassword)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Passwords do not match.");
            }

            if (request.Password.Length < 8)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Password must contain at least 8 characters.");
            }

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(request.Token)));

            var accountToken = await _authRepository
                .GetAccountTokenByHashAsync(
                    tokenHash,
                    AccountTokenType.AccountSetup);

            if (accountToken == null ||
                accountToken.UsedAtUtc != null ||
                accountToken.InvalidatedAtUtc != null ||
                accountToken.ExpiresAtUtc <= DateTime.UtcNow)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Invalid or expired setup token.");
            }

            var employee = accountToken.Employee;

            if (!employee.IsActive)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "This employee account is inactive.");
            }

            employee.PasswordHash =
                PasswordHasher.EncryptPassword(request.Password);

            accountToken.UsedAtUtc = DateTime.UtcNow;

            await _authRepository.UpdateEmployeeAsync(employee);

            await _authRepository.UpdateAccountTokenAsync(accountToken);

            await _authRepository.SaveChangesAsync();

            await LogAuditAsync(
                employee.Id,
                "AccountSetup",
                "Employee completed account setup.");

            _logger.LogInformation(
                "Employee completed account setup. EmployeeId: {EmployeeId}",
                employee.Id);

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Account setup completed successfully. You can now log in.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during account setup.");

            return ApiResponse.ResponseHelper.FailureResponse(
                "An unexpected error occurred. Please try again later.");
        } 
    }

    public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            var employee = await _authRepository
                .GetEmployeeByEmailAsync(request.Email);

            if (employee == null || !employee.IsActive)
            {
                return ApiResponse.ResponseHelper.SuccessResponse(
                    null,
                    "If an eligible account exists, password reset instructions will be sent.");
            }

            var existingTokens = await _authRepository
                .GetAccountTokensByEmployeeIdAsync(
                    employee.Id,
                    AccountTokenType.PasswordReset);

            foreach (var existingToken in existingTokens)
            {
                existingToken.InvalidatedAtUtc = DateTime.UtcNow;

                await _authRepository.UpdateAccountTokenAsync(
                    existingToken);
            }

            var rawToken = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(rawToken)));

            var accountToken = new AccountToken
            {
                EmployeeId = employee.Id,
                TokenHash = tokenHash,
                TokenType = AccountTokenType.PasswordReset,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            };

            await _authRepository.AddAccountTokenAsync(accountToken);
            await _authRepository.SaveChangesAsync();

            try
            {
                await _emailService.SendEmailAsync(
                    employee.Email,
                    "HRDirect Password Reset",
                    $"Your password reset token is: {rawToken}\n\n" +
                    "This token expires in 15 minutes. " +
                    "If you did not request a password reset, ignore this email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send password reset email.");

                accountToken.InvalidatedAtUtc = DateTime.UtcNow;

                await _authRepository.UpdateAccountTokenAsync(
                    accountToken);

                await _authRepository.SaveChangesAsync();
            }

            await LogAuditAsync(
                employee.Id,
                "ForgotPassword",
                "Password reset requested.");

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "If an eligible account exists, password reset instructions will be sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred during the forgot-password process.");

            return ApiResponse.ResponseHelper.FailureResponse(
                "An unexpected error occurred. Please try again later.");
        }    }

    public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
         try
         {
             if (string.IsNullOrWhiteSpace(request.Token))
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Reset token is required.");
             }

             if (string.IsNullOrWhiteSpace(request.NewPassword))
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "New password is required.");
             }

             if (request.NewPassword.Length < 8)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Password must contain at least 8 characters.");
             }

             if (request.NewPassword != request.ConfirmPassword)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Passwords do not match.");
             }

             var tokenHash = Convert.ToHexString(
                 SHA256.HashData(
                     Encoding.UTF8.GetBytes(request.Token)));

             var accountToken = await _authRepository
                 .GetAccountTokenByHashAsync(
                     tokenHash,
                     AccountTokenType.PasswordReset);

             if (accountToken == null ||
                 accountToken.UsedAtUtc != null ||
                 accountToken.InvalidatedAtUtc != null ||
                 accountToken.ExpiresAtUtc <= DateTime.UtcNow)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "Invalid or expired reset token.");
             }

             var employee = accountToken.Employee;

             if (!employee.IsActive)
             {
                 return ApiResponse.ResponseHelper.FailureResponse(
                     "This employee account is inactive.");
             }

             employee.PasswordHash =
                 PasswordHasher.EncryptPassword(request.NewPassword);

             employee.FailedLoginAttempts = 0;
             employee.FailedLoginWindowStartUtc = null;
             employee.LockoutEndUtc = null;

             accountToken.UsedAtUtc = DateTime.UtcNow;

             await _authRepository.UpdateEmployeeAsync(employee);

             await _authRepository.UpdateAccountTokenAsync(
                 accountToken);

             await _authRepository.SaveChangesAsync();

             await LogAuditAsync(
                 employee.Id,
                 "PasswordReset",
                 "Employee reset their password.");

             _logger.LogInformation(
                 "Password reset completed. EmployeeId: {EmployeeId}",
                 employee.Id);

             return ApiResponse.ResponseHelper.SuccessResponse(
                 null,
                 "Password reset successfully. You can now log in.");
         }
         catch (Exception ex)
         {
             _logger.LogError(
                 ex,
                 "An error occurred during password reset.");

             return ApiResponse.ResponseHelper.FailureResponse(
                 "An unexpected error occurred. Please try again later.");
         }
    }

    public async Task<ApiResponse> ChangePasswordAsync(int employeeId, ChangePasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Current password is required.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "New password is required.");
            }
            

            if (request.NewPassword != request.ConfirmPassword)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "New password and confirmation do not match.");
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Your new password must be different from your current password.");
            }

            var employee = await _authRepository
                .GetEmployeeByIdAsync(employeeId);

            if (employee == null || !employee.IsActive)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Employee account could not be found.");
            }

            var isCurrentPasswordValid =
                PasswordHasher.VerifyHashedPassword(
                    request.CurrentPassword,
                    employee.PasswordHash);

            if (!isCurrentPasswordValid)
            {
                await LogAuditAsync(
                    employee.Id,
                    "FailedPasswordChange",
                    "Incorrect current password supplied.");

                return ApiResponse.ResponseHelper.FailureResponse(
                    "Current password is incorrect.");
            }

            employee.PasswordHash =
                PasswordHasher.EncryptPassword(request.NewPassword);

            await _authRepository.UpdateEmployeeAsync(employee);
            await _authRepository.SaveChangesAsync();

            await LogAuditAsync(
                employee.Id,
                "PasswordChanged",
                "Employee changed their password.");

            _logger.LogInformation(
                "Employee changed their password. EmployeeId: {EmployeeId}",
                employee.Id);

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Password changed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while changing the password.");

            return ApiResponse.ResponseHelper.FailureResponse(
                "An unexpected error occurred. Please try again later.");
        }
    }

    private async Task LogAuditAsync(
        int? employeeId,
        string action,
        string description)
    {
        var auditLog = new AuditLog
        {
            EmployeeId = employeeId,
            Action = action,
            Description = description,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(auditLog);
    }
    
}
