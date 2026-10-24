using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRDirectAPI.Repositories.Implementations;

public class EmployeeRepository(ApplicationDbContext context) : IEmployeeRepository
{
    private readonly ApplicationDbContext _context = context;
    
    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        return await _context.Employees
            .Include(e => e.EmployeeRoleAssignments)
            .ThenInclude(era => era.Role)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByEmployeeNumberAsync(string employeeNumber)
    {
        return await _context.Employees
            .Include(e => e.EmployeeRoleAssignments)
            .ThenInclude(era => era.Role)
            .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber);
    }

    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == roleName);
    }

    public async Task<Employee?> GetEmployeeByIdWithRolesAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.EmployeeRoleAssignments)
            .ThenInclude(era => era.Role)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}