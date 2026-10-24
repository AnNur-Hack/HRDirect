namespace HRDirectAPI.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public int? EmployeeId { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Employee? Employee { get; set; }
}