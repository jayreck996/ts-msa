using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using QuizApi.Models;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BadgesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Badges.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var badge = await db.Badges.FindAsync(id);
        return badge is null ? NotFound() : Ok(badge);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId) =>
        Ok(await db.UserBadges
            .Where(ub => ub.UserId == userId)
            .Include(ub => ub.Badge)
            .ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Badge badge)
    {
        db.Badges.Add(badge);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = badge.Id }, badge);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var badge = await db.Badges.FindAsync(id);
        if (badge is null) return NotFound();
        db.Badges.Remove(badge);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
