namespace HRDirectAPI.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Title { get; set; } 

    public string Message { get; set; } 

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}