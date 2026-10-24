using HRDirectAPI.Domain.Entities;

namespace HRDirectAPI.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);

}