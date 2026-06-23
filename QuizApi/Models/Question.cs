namespace QuizApi.Models;

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
    public int? CorrectOptionId { get; set; }

    public Quiz Quiz { get; set; } = null!;
    public Option? CorrectOption { get; set; }
    public ICollection<Option> Options { get; set; } = [];
}
