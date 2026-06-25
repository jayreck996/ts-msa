namespace QuizApi.Models;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Easy"; // Easy | Medium | Hard
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Category? Category { get; set; }
    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<QuizAttempt> Attempts { get; set; } = [];
}