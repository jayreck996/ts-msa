using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTop([FromQuery] int top = 10)
    {
        var leaders = await db.Users
            .OrderByDescending(u => u.TotalPoints)
            .Take(top)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.TotalPoints,
                u.Level,
                u.CurrentStreak
            })
            .ToListAsync();
        return Ok(leaders);
    }
}
