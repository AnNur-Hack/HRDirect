namespace HRDirectAPI.Domain.Entities;

public class Lesson
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string MaterialUrl { get; set; } = string.Empty;

    public int OrderNumber { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}