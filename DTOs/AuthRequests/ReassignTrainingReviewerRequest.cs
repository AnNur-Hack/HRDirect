namespace HRDirectAPI.DTOs.AuthRequests;

public class ReassignTrainingReviewerRequest
{
    public int NewReviewerEmployeeId { get; set; }
    public string Reason { get; set; }
}