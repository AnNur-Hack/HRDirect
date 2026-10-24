namespace HRDirectAPI.Domain.Entities;

public class AssessmentAttempt
{
    public int Id { get; set; }

    public int LearningAssignmentId { get; set; }

    public LearningAssignment LearningAssignment { get; set; } = null!;

    public int AttemptNumber { get; set; }

    public int CycleNumber { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;
}