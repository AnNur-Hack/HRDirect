namespace HRDirectAPI.DTOs.AuthRequests;

public class CreateOptionRequest
{
    public string OptionText { get; set; } 

    public bool IsCorrect { get; set; }
}