using HRDirectAPI.Domain.Entities;

namespace HRDirectAPI.Repositories.Interfaces;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllEmployeesAsync();

    Task<Employee?> GetEmployeeByEmployeeNumberAsync(
        string employeeNumber);

    Task<Role?> GetRoleByNameAsync(string roleName);

    Task<Employee?> GetEmployeeByIdWithRolesAsync(int id);
}