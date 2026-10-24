using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class LearningAssignment
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public int EmployeeId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime DueDateUtc { get; set; }

    public LearningAssignmentStatus Status { get; set; } =
        LearningAssignmentStatus.NotStarted;

    public int CurrentCycle { get; set; } = 1;

    public decimal? BestScore { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public ICollection<LessonCompletion> LessonCompletions { get; set; } =
        new List<LessonCompletion>();

    public ICollection<AssessmentAttempt> AssessmentAttempts { get; set; } =
        new List<AssessmentAttempt>();

    public ICollection<AssignmentResetHistory> ResetHistory { get; set; } =
        new List<AssignmentResetHistory>();
}