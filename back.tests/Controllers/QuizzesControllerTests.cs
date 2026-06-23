using Microsoft.AspNetCore.Mvc;
using QuizApi.Controllers;
using QuizApi.Models;
using QuizApi.Tests.Helpers;

namespace QuizApi.Tests.Controllers;

public class QuizzesControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedQuiz()
    {
        using var db = DbHelper.CreateDb();
        db.Categories.Add(new Category { Id = 1, Name = "Tech" });
        await db.SaveChangesAsync();

        var controller = new QuizzesController(db);
        var quiz = new Quiz { Title = "C# Basics", Description = "Intro", Difficulty = "Easy", CategoryId = 1 };

        var result = await controller.Create(quiz) as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.Equal("C# Basics", (result.Value as Quiz)!.Title);
    }

    [Fact]
    public async Task GetAll_FiltersByDifficulty()
    {
        using var db = DbHelper.CreateDb();
        db.Categories.Add(new Category { Id = 1, Name = "Tech" });
        db.Quizzes.AddRange(
            new Quiz { Title = "Easy Quiz", Difficulty = "Easy", CategoryId = 1 },
            new Quiz { Title = "Hard Quiz", Difficulty = "Hard", CategoryId = 1 }
        );
        await db.SaveChangesAsync();

        var controller = new QuizzesController(db);
        var result = await controller.GetAll("Easy", null) as OkObjectResult;
        var list = (result!.Value as IEnumerable<Quiz>)!.ToList();

        Assert.Single(list);
        Assert.Equal("Easy Quiz", list[0].Title);
    }

    [Fact]
    public async Task Delete_RemovesQuiz()
    {
        using var db = DbHelper.CreateDb();
        db.Categories.Add(new Category { Id = 1, Name = "Tech" });
        await db.SaveChangesAsync();
        var controller = new QuizzesController(db);
        var quiz = new Quiz { Title = "To Delete", Difficulty = "Easy", CategoryId = 1 };
        await controller.Create(quiz);

        var result = await controller.Delete(quiz.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, db.Quizzes.Count());
    }
}
