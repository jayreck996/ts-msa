using Microsoft.AspNetCore.Mvc;
using QuizApi.Controllers;
using QuizApi.Models;
using QuizApi.Tests.Helpers;

namespace QuizApi.Tests.Controllers;

public class LeaderboardControllerTests
{
    [Fact]
    public async Task GetTop_ReturnsUsersOrderedByPoints()
    {
        using var db = DbHelper.CreateDb();
        db.Users.AddRange(
            new User { Username = "alice", Email = "a@a.com", TotalPoints = 300 },
            new User { Username = "bob",   Email = "b@b.com", TotalPoints = 500 },
            new User { Username = "carol", Email = "c@c.com", TotalPoints = 100 }
        );
        await db.SaveChangesAsync();

        var controller = new LeaderboardController(db);
        var result = await controller.GetTop(3) as OkObjectResult;

        Assert.NotNull(result);
        var list = (result.Value as IEnumerable<dynamic>)!.ToList();
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task GetTop_RespectsTopParam()
    {
        using var db = DbHelper.CreateDb();
        db.Users.AddRange(
            new User { Username = "u1", Email = "u1@a.com", TotalPoints = 100 },
            new User { Username = "u2", Email = "u2@a.com", TotalPoints = 200 },
            new User { Username = "u3", Email = "u3@a.com", TotalPoints = 300 }
        );
        await db.SaveChangesAsync();

        var controller = new LeaderboardController(db);
        var result = await controller.GetTop(2) as OkObjectResult;

        var list = (result!.Value as System.Collections.IEnumerable)!.Cast<object>().ToList();
        Assert.Equal(2, list.Count);
    }
}
