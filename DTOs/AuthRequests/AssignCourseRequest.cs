namespace HRDirectAPI.DTOs.AuthRequests;

public class AssignCourseRequest
{
    public int CourseId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime DueDateUtc { get; set; }
}