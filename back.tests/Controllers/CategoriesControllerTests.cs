using Microsoft.AspNetCore.Mvc;
using QuizApi.Controllers;
using QuizApi.Models;
using QuizApi.Tests.Helpers;

namespace QuizApi.Tests.Controllers;

public class CategoriesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoCategories()
    {
        using var db = DbHelper.CreateDb();
        var controller = new CategoriesController(db);
        var result = await controller.GetAll() as OkObjectResult;
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IEnumerable<Category>>(result.Value);
    }

    [Fact]
    public async Task Create_AddsCategory_AndReturnsCreated()
    {
        using var db = DbHelper.CreateDb();
        var controller = new CategoriesController(db);
        var category = new Category { Name = "Science" };

        var result = await controller.Create(category) as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        var created = result.Value as Category;
        Assert.Equal("Science", created!.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var db = DbHelper.CreateDb();
        var controller = new CategoriesController(db);
        var result = await controller.GetById(999);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_RemovesCategory()
    {
        using var db = DbHelper.CreateDb();
        var controller = new CategoriesController(db);
        var category = new Category { Name = "Tech" };
        await controller.Create(category);

        var result = await controller.Delete(category.Id);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, db.Categories.Count());
    }

    [Fact]
    public async Task Update_ChangesName()
    {
        using var db = DbHelper.CreateDb();
        var controller = new CategoriesController(db);
        var category = new Category { Name = "Math" };
        await controller.Create(category);

        var result = await controller.Update(category.Id, new Category { Name = "Advanced Math" }) as OkObjectResult;

        Assert.NotNull(result);
        var updated = result.Value as Category;
        Assert.Equal("Advanced Math", updated!.Name);
    }
}
