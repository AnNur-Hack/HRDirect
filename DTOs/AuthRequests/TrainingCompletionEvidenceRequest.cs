namespace HRDirectAPI.DTOs.AuthRequests;

public class TrainingCompletionEvidenceRequest
{
    public IFormFile? EvidenceFile { get; set; }

    public string Comment { get; set; } 
}