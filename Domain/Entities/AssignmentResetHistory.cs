namespace HRDirectAPI.Domain.Entities;

public class AssignmentResetHistory
{
    public int Id { get; set; }

    public int LearningAssignmentId { get; set; }

    public LearningAssignment LearningAssignment { get; set; } = null!;

    public int ResetByEmployeeId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int PreviousCycle { get; set; }

    public int NewCycle { get; set; }

    public DateTime ResetAtUtc { get; set; } = DateTime.UtcNow;
}