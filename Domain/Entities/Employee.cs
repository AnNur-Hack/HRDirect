namespace HRDirectAPI.Domain.Entities;

public class Employee
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string Department { get; set; }

    public string Grade { get; set; }

    public int? ReportingManagerId { get; set; }

    public Employee? ReportingManager { get; set; }

    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    public string PasswordHash { get; set; }

    public bool IsActive { get; set; }
    
    public int FailedLoginAttempts { get; set; }


    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; set; }
    
    public DateTime? FailedLoginWindowStartUtc { get; set; }

    public DateTime? LockoutEndUtc { get; set; }

    public ICollection<AccountToken> AccountTokens { get; set; }
        = new List<AccountToken>();
    
    public ICollection<EmployeeRoleAssignment> EmployeeRoleAssignments { get; set; }
        = new List<EmployeeRoleAssignment>();
}