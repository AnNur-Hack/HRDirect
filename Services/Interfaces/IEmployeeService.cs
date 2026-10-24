using HRDirectAPI.DTOs.AuthRequests;
using HRDirectAPI.DTOs.Responses;

namespace HRDirectAPI.Services.Interfaces;

public interface IEmployeeService
{
    Task<ApiResponse> CreateEmployeeAsync(CreateEmployeeRequest request);

    Task<ApiResponse> GetAllEmployeesAsync();

    Task<ApiResponse> GetEmployeeByEmployeeNumberAsync(string employeeNumber);
    
    Task<ApiResponse> UpdateEmployeeAsync(
        string employeeNumber,
        UpdateEmployeeRequest request);

    Task<ApiResponse> DeactivateEmployeeAsync(
        string employeeNumber,
        int adminEmployeeId);
}