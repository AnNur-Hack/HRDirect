using System.Security.Cryptography;
using System.Text;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;
using HRDirectAPI.Repositories.Interfaces;
using HRDirectAPI.Services.Interfaces;

namespace HRDirectAPI.Services.Implementations;

public class EmployeeService(IEmployeeRepository employeeRepository, 
    IAuthRepository authRepository, IEmailService emailService,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IAuthRepository _authRepository = authRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<EmployeeService> _logger = logger;
    
    
    public async Task<ApiResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        
        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var employeeNumber = request.EmployeeNumber.Trim();

            if (await _authRepository.EmailExistsAsync(email))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "An employee with this email already exists.");
            }

            if (await _authRepository.EmployeeNumberExistsAsync(employeeNumber))
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "This employee number already exists.");
            }

            if (request.ReportingManagerId.HasValue)
            {
                var manager = await _authRepository.GetEmployeeByIdAsync(
                    request.ReportingManagerId.Value);

                if (manager == null || !manager.IsActive)
                {
                    return ApiResponse.ResponseHelper.FailureResponse(
                        "The selected reporting manager does not exist or is inactive.");
                }
            }

            if (request.Roles == null || request.Roles.Count == 0)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "At least one role must be assigned.");
            }

            var roleNames = request.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Select(role => role.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (roleNames.Count == 0)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "At least one valid role must be assigned.");
            }

            var roles = new List<Role>();

            foreach (var roleName in roleNames)
            {
                var role = await _employeeRepository.GetRoleByNameAsync(
                    roleName);

                if (role == null)
                {
                    return ApiResponse.ResponseHelper.FailureResponse(
                        $"The role '{roleName}' does not exist.");
                }

                roles.Add(role);
            }

            var setupToken = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            var tokenHash = Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(setupToken)));

            var currentTime = DateTime.UtcNow;

            var employee = new Employee
            {
                EmployeeNumber = employeeNumber,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                PhoneNumber = request.PhoneNumber,
                Department = request.Department.Trim(),
                Grade = request.Grade.Trim(),
                ReportingManagerId = request.ReportingManagerId,
                PasswordHash = string.Empty,
                IsActive = true,
                CreatedAtUtc = currentTime
            };

            foreach (var role in roles)
            {
                employee.EmployeeRoleAssignments.Add(
                    new EmployeeRoleAssignment
                    {
                        Employee = employee,
                        RoleId = role.Id
                    });
            }

            var accountToken = new AccountToken
            {
                Employee = employee,
                TokenHash = tokenHash,
                TokenType = AccountTokenType.AccountSetup,
                CreatedAtUtc = currentTime,
                ExpiresAtUtc = currentTime.AddHours(24)
            };

            await _authRepository.AddEmployeeAsync(employee);
            await _authRepository.AddAccountTokenAsync(accountToken);
            await _authRepository.SaveChangesAsync();

            try
            {
                var message =
                    $"Hello {employee.FirstName},\n\n" +
                    "Your HRDirect account has been created.\n\n" +
                    $"Your account-setup token is:\n{setupToken}\n\n" +
                    "Use this token to set up your password. " +
                    "It expires in 24 hours.\n\n" +
                    "If you did not expect this invitation, " +
                    "please contact HR.";

                await _emailService.SendEmailAsync(
                    employee.Email,
                    "HRDirect Account Setup",
                    message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send account-setup email to employee {EmployeeId}.",
                    employee.Id);

                return ApiResponse.ResponseHelper.SuccessResponse(
                    MapEmployeeResponse(employee),
                    "Employee created, but the invitation email could not be sent. Please arrange to resend the invitation.");
            }

            return ApiResponse.ResponseHelper.SuccessResponse(
                MapEmployeeResponse(employee),
                "Employee created successfully. An account-setup invitation has been sent.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating an employee.");

            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while creating the employee.");
        }
    }

    public async Task<ApiResponse> GetAllEmployeesAsync()
    {
        try
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            var result = employees
                .Select(MapEmployeeResponse)
                .ToList();

            return ApiResponse.ResponseHelper.SuccessResponse(
                result,
                "Employees retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve employees.");

            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while retrieving employees.");
        }
    }

    public async Task<ApiResponse> GetEmployeeByEmployeeNumberAsync(
        string employeeNumber)
    {
        try
        {
            var employee = await _employeeRepository
                .GetEmployeeByEmployeeNumberAsync(employeeNumber);

            if (employee == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Employee not found.");
            }

            return ApiResponse.ResponseHelper.SuccessResponse(
                MapEmployeeResponse(employee),
                "Employee retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to retrieve employee {EmployeeNumber}.",
                employeeNumber);

            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while retrieving the employee.");
        }
    }

    public async Task<ApiResponse> UpdateEmployeeAsync(string employeeNumber, UpdateEmployeeRequest request)
    {
        try
        {
            var employee = await _employeeRepository
                .GetEmployeeByEmployeeNumberAsync(employeeNumber);

            if (employee == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Employee not found.");
            }

            if (request.ReportingManagerId.HasValue)
            {
                if (request.ReportingManagerId.Value == employee.Id)
                {
                    return ApiResponse.ResponseHelper.FailureResponse(
                        "An employee cannot be their own reporting manager.");
                }

                var manager = await _authRepository.GetEmployeeByIdAsync(
                    request.ReportingManagerId.Value);

                if (manager == null || !manager.IsActive)
                {
                    return ApiResponse.ResponseHelper.FailureResponse(
                        "The selected reporting manager does not exist or is inactive.");
                }
            }

            employee.FirstName = request.FirstName.Trim();
            employee.LastName = request.LastName.Trim();
            employee.PhoneNumber = request.PhoneNumber;
            employee.Department = request.Department.Trim();
            employee.Grade = request.Grade.Trim();
            employee.ReportingManagerId = request.ReportingManagerId;

            await _authRepository.UpdateEmployeeAsync(employee);
            await _authRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                MapEmployeeResponse(employee),
                "Employee updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update employee {EmployeeNumber}.",
                employeeNumber);

            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while updating the employee.");
        }
    }

    public async Task<ApiResponse> DeactivateEmployeeAsync(string employeeNumber, int adminEmployeeId)
    {
        try
        {
            var employee = await _employeeRepository
                .GetEmployeeByEmployeeNumberAsync(employeeNumber);

            if (employee == null)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "Employee not found.");
            }

            if (employee.Id == adminEmployeeId)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "You cannot deactivate your own account.");
            }

            if (!employee.IsActive)
            {
                return ApiResponse.ResponseHelper.FailureResponse(
                    "This employee is already inactive.");
            }

            employee.IsActive = false;

            await _authRepository.UpdateEmployeeAsync(employee);
            await _authRepository.SaveChangesAsync();

            return ApiResponse.ResponseHelper.SuccessResponse(
                null,
                "Employee deactivated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to deactivate employee {EmployeeNumber}.",
                employeeNumber);

            return ApiResponse.ResponseHelper.FailureResponse(
                "An error occurred while deactivating the employee.");
        }
    }

    private static EmployeeResponse MapEmployeeResponse(Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            Department = employee.Department,
            Grade = employee.Grade,
            Roles = employee.EmployeeRoleAssignments
                .Select(assignment => assignment.Role.Name)
                .ToList()
        };
    }
}
