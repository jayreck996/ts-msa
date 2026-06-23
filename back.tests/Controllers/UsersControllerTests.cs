using Microsoft.AspNetCore.Mvc;
using QuizApi.Controllers;
using QuizApi.Models;
using QuizApi.Tests.Helpers;

namespace QuizApi.Tests.Controllers;

public class UsersControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedUser()
    {
        using var db = DbHelper.CreateDb();
        var controller = new UsersController(db);
        var user = new User { Username = "jay", Email = "jay@test.com" };

        var result = await controller.Create(user) as CreatedAtActionResult;

        Assert.NotNull(result);
        var created = result.Value as User;
        Assert.Equal("jay", created!.Username);
        Assert.Equal(0, created.TotalPoints);
        Assert.Equal(0, created.Level);
    }

    [Fact]
    public async Task GetById_ReturnsUser_WhenExists()
    {
        using var db = DbHelper.CreateDb();
        var controller = new UsersController(db);
        var user = new User { Username = "jay", Email = "jay@test.com" };
        await controller.Create(user);

        var result = await controller.GetById(user.Id) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(user.Id, (result.Value as User)!.Id);
    }

    [Fact]
    public async Task Delete_RemovesUser()
    {
        using var db = DbHelper.CreateDb();
        var controller = new UsersController(db);
        var user = new User { Username = "temp", Email = "t@t.com" };
        await controller.Create(user);

        await controller.Delete(user.Id);

        Assert.Equal(0, db.Users.Count());
    }
}
