namespace HRDirectAPI.Domain.Entities;

public class QuizOption
{
    public int Id { get; set; }

    public int QuizQuestionId { get; set; }

    public QuizQuestion QuizQuestion { get; set; } = null!;

    public string OptionText { get; set; } 

    public bool IsCorrect { get; set; }
}