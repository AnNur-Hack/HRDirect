using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<Employee?> GetEmployeeByEmailAsync(string email);

    Task<Employee?> GetEmployeeByIdAsync(int id);

    Task<bool> EmailExistsAsync(string email);

    Task<bool> EmployeeNumberExistsAsync(string employeeNumber);

    Task AddEmployeeAsync(Employee employee);
    
    Task<AccountToken?> GetAccountTokenByHashAsync(
        string tokenHash,
        AccountTokenType tokenType);
    
    Task AddAccountTokenAsync(AccountToken accountToken);

    Task<List<AccountToken>> GetAccountTokensByEmployeeIdAsync(
        int employeeId,
        AccountTokenType tokenType);
    
    Task UpdateAccountTokenAsync(AccountToken accountToken);
    Task UpdateEmployeeAsync(Employee employee);

    Task SaveChangesAsync();
}