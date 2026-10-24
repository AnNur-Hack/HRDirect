namespace HRDirectAPI.Domain.Entities;

public class LessonCompletion
{
    public int Id { get; set; }

    public int LearningAssignmentId { get; set; }

    public LearningAssignment LearningAssignment { get; set; } = null!;

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public DateTime CompletedAtUtc { get; set; } = DateTime.UtcNow;
}
