namespace HRDirectAPI.Domain.Entities;

public class EmailQueue
{
    public int Id { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;

    public string Subject { get; set; } 

    public string Body { get; set; } 

    public bool IsSent { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? SentAtUtc { get; set; }
}