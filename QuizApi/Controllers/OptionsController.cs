using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using QuizApi.Models;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OptionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetByQuestion([FromQuery] int questionId) =>
        Ok(await db.Options.Where(o => o.QuestionId == questionId).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var option = await db.Options.FindAsync(id);
        return option is null ? NotFound() : Ok(option);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Option option)
    {
        db.Options.Add(option);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = option.Id }, option);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Option updated)
    {
        var option = await db.Options.FindAsync(id);
        if (option is null) return NotFound();
        option.Text = updated.Text;
        await db.SaveChangesAsync();
        return Ok(option);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var option = await db.Options.FindAsync(id);
        if (option is null) return NotFound();
        db.Options.Remove(option);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
