namespace QuizApi.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Quiz> Quizzes { get; set; } = [];
}
