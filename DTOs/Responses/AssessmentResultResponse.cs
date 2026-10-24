namespace HRDirectAPI.DTOs.Responses;

public class AssessmentResultResponse
{
    public int AssignmentId { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public int AttemptNumber { get; set; }

    public int AttemptsRemaining { get; set; }

    public string Status { get; set; } 
}