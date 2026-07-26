using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using QuizApi.Models;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttemptsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByUser([FromQuery] int userId) =>
        Ok(await db.QuizAttempts
            .Where(a => a.UserId == userId)
            .Include(a => a.Quiz)
            .ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var attempt = await db.QuizAttempts.FindAsync(id);
        return attempt is null ? NotFound() : Ok(attempt);
    }

    [HttpPost]
    public async Task<IActionResult> Submit(QuizAttempt attempt)
    {
        var quiz = await db.Quizzes.FindAsync(attempt.QuizId);
        if (quiz is null) return BadRequest("Quiz not found");

        attempt.PointsEarned = quiz.Difficulty switch
        {
            "Medium" => attempt.Score * 20,
            "Hard"   => attempt.Score * 30,
            _        => attempt.Score * 10
        };

        db.QuizAttempts.Add(attempt);

        var user = await db.Users.FindAsync(attempt.UserId);
        if (user is not null)
        {
            user.TotalPoints += attempt.PointsEarned;
            user.Level = user.TotalPoints / 100;

            var today = DateTime.UtcNow.Date;
            var lastAttempt = await db.QuizAttempts
                .Where(a => a.UserId == user.Id && a.Id != attempt.Id)
                .OrderByDescending(a => a.CompletedAt)
                .FirstOrDefaultAsync();

            if (lastAttempt is null || lastAttempt.CompletedAt.Date < today.AddDays(-1))
                user.CurrentStreak = 1;
            else if (lastAttempt.CompletedAt.Date == today.AddDays(-1))
                user.CurrentStreak++;

            if (user.CurrentStreak > user.LongestStreak)
                user.LongestStreak = user.CurrentStreak;
        }

        // Persist the attempt + user stats first so badge checks below (which query
        // the DB for attempt counts / perfect scores) see this attempt as already saved.
        await db.SaveChangesAsync();

        if (user is not null)
        {
            await AwardBadges(user);
            await db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = attempt.Id }, attempt);
    }

    private async Task AwardBadges(User user)
    {
        var earned = await db.UserBadges.Where(ub => ub.UserId == user.Id).Select(ub => ub.BadgeId).ToListAsync();
        var allBadges = await db.Badges.ToListAsync();

        foreach (var badge in allBadges)
        {
            if (earned.Contains(badge.Id)) continue;

            var grant = badge.Requirement switch
            {
                "first_quiz"    => await db.QuizAttempts.CountAsync(a => a.UserId == user.Id) >= 1,
                "points_100"    => user.TotalPoints >= 100,
                "points_500"    => user.TotalPoints >= 500,
                "streak_7"      => user.CurrentStreak >= 7,
                "perfect_score" => await db.QuizAttempts.AnyAsync(a => a.UserId == user.Id && a.Score == 10),
                _               => false
            };

            if (grant)
                db.UserBadges.Add(new UserBadge { UserId = user.Id, BadgeId = badge.Id });
        }
    }
}
