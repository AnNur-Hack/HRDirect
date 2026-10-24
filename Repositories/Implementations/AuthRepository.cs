using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Domain.Enums;
using HRDirectAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRDirectAPI.Repositories.Implementations;

public class AuthRepository(ApplicationDbContext context) : IAuthRepository
{
    private readonly ApplicationDbContext _context = context;
    
    public async Task<Employee?> GetEmployeeByEmailAsync(string email)
    {
        return await _context.Employees
            .Include(e => e.EmployeeRoleAssignments)
            .ThenInclude(era => era.Role)
            .FirstOrDefaultAsync(e => e.Email == email);    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.EmployeeRoleAssignments)
            .ThenInclude(era => era.Role)
            .FirstOrDefaultAsync(e => e.Id == id);    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Employees
            .AnyAsync(e => e.Email == email);    }

    public async Task<bool> EmployeeNumberExistsAsync(string employeeNumber)
    {
        return await _context.Employees
            .AnyAsync(e => e.EmployeeNumber == employeeNumber);    }

    public async Task AddEmployeeAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
    }

    public async Task<AccountToken?> GetAccountTokenByHashAsync(string tokenHash, AccountTokenType tokenType)
    {
        return await _context.AccountTokens
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t =>
                t.TokenHash == tokenHash &&
                t.TokenType == tokenType);
    }

    public async Task AddAccountTokenAsync(AccountToken accountToken)
    {
        await _context.AccountTokens.AddAsync(accountToken);

    }

    public async Task<List<AccountToken>> GetAccountTokensByEmployeeIdAsync(int employeeId, AccountTokenType tokenType)
    {
        return await _context.AccountTokens
            .Where(t =>
                t.EmployeeId == employeeId &&
                t.TokenType == tokenType &&
                t.UsedAtUtc == null &&
                t.InvalidatedAtUtc == null)
            .ToListAsync();    }

    public Task UpdateAccountTokenAsync(AccountToken accountToken)
    {
        _context.AccountTokens.Update(accountToken);

        return Task.CompletedTask;    
    }

    public Task UpdateEmployeeAsync(Employee employee)
    {
        _context.Employees.Update(employee);

        return Task.CompletedTask;
        
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}