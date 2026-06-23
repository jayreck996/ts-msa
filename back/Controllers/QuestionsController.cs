using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using QuizApi.Models;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByQuiz([FromQuery] int quizId)
    {
        var questions = await db.Questions
            .Where(q => q.QuizId == quizId)
            .Include(q => q.Options)
            .ToListAsync();
        return Ok(questions);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var question = await db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == id);
        return question is null ? NotFound() : Ok(question);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Question question)
    {
        db.Questions.Add(question);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Question updated)
    {
        var question = await db.Questions.FindAsync(id);
        if (question is null) return NotFound();
        question.Text = updated.Text;
        question.Points = updated.Points;
        question.CorrectOptionId = updated.CorrectOptionId;
        await db.SaveChangesAsync();
        return Ok(question);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var question = await db.Questions.FindAsync(id);
        if (question is null) return NotFound();
        db.Questions.Remove(question);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
