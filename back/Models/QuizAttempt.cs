namespace QuizApi.Models;

public class QuizAttempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QuizId { get; set; }
    public int Score { get; set; }
    public int PointsEarned { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Quiz? Quiz { get; set; }
}