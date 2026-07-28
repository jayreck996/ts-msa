using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuizApi.Controllers;
using QuizApi.Models;
using QuizApi.Tests.Helpers;

namespace QuizApi.Tests.Controllers;

public class AttemptsControllerTests
{
    private static AttemptsController ControllerFor(QuizApi.Data.AppDbContext db, int userId)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "test");
        var controller = new AttemptsController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) },
            },
        };
        return controller;
    }

    private static async Task<(User user, Quiz quiz)> SeedUserAndQuiz(QuizApi.Data.AppDbContext db, string difficulty = "Hard")
    {
        var user = new User { Username = "jay", Email = "jay@test.com" };
        var quiz = new Quiz { Title = "Q", Difficulty = difficulty };
        db.Users.Add(user);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();
        return (user, quiz);
    }

    [Fact]
    public async Task Submit_ComputesPointsByDifficulty()
    {
        using var db = DbHelper.CreateDb();
        var (user, quiz) = await SeedUserAndQuiz(db, "Hard");
        var controller = ControllerFor(db, user.Id);

        var result = await controller.Submit(new QuizAttempt { QuizId = quiz.Id, Score = 3, CompletedAt = DateTime.UtcNow })
            as CreatedAtActionResult;

        Assert.NotNull(result);
        var attempt = result.Value as QuizAttempt;
        Assert.Equal(90, attempt!.PointsEarned); // 3 * 30 (Hard)
    }

    [Fact]
    public async Task Submit_IgnoresClientSuppliedUserId_UsesAuthenticatedUser()
    {
        using var db = DbHelper.CreateDb();
        var (user, quiz) = await SeedUserAndQuiz(db);
        var attacker = new User { Username = "attacker", Email = "attacker@test.com" };
        db.Users.Add(attacker);
        await db.SaveChangesAsync();

        var controller = ControllerFor(db, user.Id);

        // Body claims to submit on behalf of "attacker" — must be ignored.
        await controller.Submit(new QuizAttempt { UserId = attacker.Id, QuizId = quiz.Id, Score = 1, CompletedAt = DateTime.UtcNow });

        Assert.Equal(0, db.QuizAttempts.Count(a => a.UserId == attacker.Id));
        Assert.Equal(1, db.QuizAttempts.Count(a => a.UserId == user.Id));
    }

    [Fact]
    public async Task Submit_AwardsFirstQuizBadge_OnTheTriggeringAttempt()
    {
        using var db = DbHelper.CreateDb();
        var (user, quiz) = await SeedUserAndQuiz(db);
        db.Badges.Add(new Badge { Name = "First Quiz", Requirement = "first_quiz" });
        await db.SaveChangesAsync();
        var controller = ControllerFor(db, user.Id);

        await controller.Submit(new QuizAttempt { QuizId = quiz.Id, Score = 1, CompletedAt = DateTime.UtcNow });

        Assert.True(db.UserBadges.Any(ub => ub.UserId == user.Id));
    }

    [Fact]
    public async Task Submit_AwardsPerfectScoreBadge_OnTheTriggeringAttempt()
    {
        using var db = DbHelper.CreateDb();
        var (user, quiz) = await SeedUserAndQuiz(db);
        db.Badges.Add(new Badge { Name = "Perfect Score", Requirement = "perfect_score" });
        await db.SaveChangesAsync();
        var controller = ControllerFor(db, user.Id);

        await controller.Submit(new QuizAttempt { QuizId = quiz.Id, Score = 10, CompletedAt = DateTime.UtcNow });

        Assert.True(db.UserBadges.Any(ub => ub.UserId == user.Id));
    }
}
