namespace HRDirectAPI.DTOs.AuthRequests;

public class SubmitAssessmentRequest
{
    public List<AssessmentAnswerRequest> Answers { get; set; } = new();

}