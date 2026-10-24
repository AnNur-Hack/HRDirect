using HRDirectAPI.Data;
using HRDirectAPI.Domain.Entities;
using HRDirectAPI.Repositories.Interfaces;

namespace HRDirectAPI.Repositories.Implementations;

public class AuditLogRepository(ApplicationDbContext context) : IAuditLogRepository
{
    private readonly ApplicationDbContext _context = context;
    
    public async Task AddAsync(AuditLog auditLog)
    {
        await _context.AuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();    }
}