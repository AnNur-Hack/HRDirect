namespace HRDirectAPI.DTOs.AuthRequests;

public class CreateCourseRequest
{
    public string Title { get; set; } 

    public string Description { get; set; } 

    public decimal PassMark { get; set; } = 70;
}