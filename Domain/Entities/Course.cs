using HRDirectAPI.Domain.Enums;

namespace HRDirectAPI.Domain.Entities;

public class Course
{
    public int Id { get; set; }

    public string Title { get; set; } 

    public string Description { get; set; } 

    public decimal PassMark { get; set; } = 70;

    public CourseStatus Status { get; set; } = CourseStatus.Draft;

    public int Version { get; set; } = 1;

    public int CreatedByEmployeeId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAtUtc { get; set; }

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public ICollection<QuizQuestion> Questions { get; set; } =
        new List<QuizQuestion>();

    public ICollection<LearningAssignment> Assignments { get; set; } =
        new List<LearningAssignment>();
}