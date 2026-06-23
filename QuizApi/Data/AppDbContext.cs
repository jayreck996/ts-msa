using Microsoft.EntityFrameworkCore;
using QuizApi.Models;

namespace QuizApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Question -> CorrectOption: no cascade to avoid cycle
        modelBuilder.Entity<Question>()
            .HasOne(q => q.CorrectOption)
            .WithMany()
            .HasForeignKey(q => q.CorrectOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Option -> Question: cascade delete options when question deleted
        modelBuilder.Entity<Option>()
            .HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
