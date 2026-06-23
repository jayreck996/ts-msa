namespace QuizApi.Models;

public class Badge
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirement { get; set; } = string.Empty;

    public ICollection<UserBadge> UserBadges { get; set; } = [];
}
