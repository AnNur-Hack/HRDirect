namespace HRDirectAPI.DTOs.Responses;

public class CourseResponse
{
    public int Id { get; set; }

    public string Title { get; set; } 

    public string Description { get; set; }

    public decimal PassMark { get; set; }

    public string Status { get; set; } 

    public int Version { get; set; }
}