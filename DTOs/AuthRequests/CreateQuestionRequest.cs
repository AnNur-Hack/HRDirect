namespace HRDirectAPI.DTOs.AuthRequests;

public class CreateQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;

    public List<CreateOptionRequest> Options { get; set; } = new();
}