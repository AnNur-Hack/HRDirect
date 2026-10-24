namespace HRDirectAPI.DTOs.AuthRequests;

public class ResetAssignmentRequest
{
    public int AssignmentId { get; set; }

    public string Reason { get; set; }
}