using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using QuizApi.Models;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizzesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? difficulty, [FromQuery] int? categoryId)
    {
        var query = db.Quizzes.Include(q => q.Category).AsQueryable();
        if (difficulty is not null) query = query.Where(q => q.Difficulty == difficulty);
        if (categoryId is not null) query = query.Where(q => q.CategoryId == categoryId);
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Category)
            .Include(q => q.Questions).ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);
        return quiz is null ? NotFound() : Ok(quiz);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Quiz quiz)
    {
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = quiz.Id }, quiz);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Quiz updated)
    {
        var quiz = await db.Quizzes.FindAsync(id);
        if (quiz is null) return NotFound();
        quiz.Title = updated.Title;
        quiz.Description = updated.Description;
        quiz.Difficulty = updated.Difficulty;
        quiz.CategoryId = updated.CategoryId;
        await db.SaveChangesAsync();
        return Ok(quiz);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await db.Quizzes.FindAsync(id);
        if (quiz is null) return NotFound();
        db.Quizzes.Remove(quiz);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
