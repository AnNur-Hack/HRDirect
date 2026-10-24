namespace HRDirectAPI.DTOs.AuthRequests;

public class CreateLessonRequest
{
    public string Title { get; set; } 

    public string MaterialUrl { get; set; } 

    public int OrderNumber { get; set; }
}