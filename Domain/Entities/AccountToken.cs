using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class AccountToken
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee Employee { get; set; }

    public string TokenHash { get; set; }

    public AccountTokenType TokenType { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }

    public DateTime? InvalidatedAtUtc { get; set; }
}