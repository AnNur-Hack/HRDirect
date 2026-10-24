namespace HRDirectAPI.Domain.Entities;

public class QuizQuestion
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string QuestionText { get; set; } 

    public ICollection<QuizOption> Options { get; set; } =
        new List<QuizOption>();
}