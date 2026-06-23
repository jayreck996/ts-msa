namespace QuizApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int TotalPoints { get; set; }
    public int Level { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<QuizAttempt> Attempts { get; set; } = [];
    public ICollection<UserBadge> UserBadges { get; set; } = [];
}
