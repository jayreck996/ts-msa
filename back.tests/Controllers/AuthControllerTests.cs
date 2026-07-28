using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using QuizApi.Controllers;
using QuizApi.Models;
using QuizApi.Tests.Helpers;

namespace QuizApi.Tests.Controllers;

public class AuthControllerTests
{
    private static IConfiguration TestConfig() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-signing-key-at-least-32-characters-long",
            ["Jwt:Issuer"] = "ts-msa-tests",
            ["Jwt:Audience"] = "ts-msa-tests-client",
        }).Build();

    [Fact]
    public async Task Register_HashesPassword_NeverStoresPlaintext()
    {
        using var db = DbHelper.CreateDb();
        var controller = new AuthController(db, TestConfig());

        var result = await controller.Register(new RegisterRequest
        {
            Username = "jay", Email = "jay@test.com", Password = "correct-horse-battery-staple",
        }) as OkObjectResult;

        Assert.NotNull(result);
        var stored = db.Users.Single(u => u.Username == "jay");
        Assert.NotEqual("correct-horse-battery-staple", stored.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("correct-horse-battery-staple", stored.PasswordHash));
    }

    [Fact]
    public async Task Register_RejectsDuplicateUsername()
    {
        using var db = DbHelper.CreateDb();
        var controller = new AuthController(db, TestConfig());
        await controller.Register(new RegisterRequest { Username = "jay", Email = "a@a.com", Password = "pw12345" });

        var result = await controller.Register(new RegisterRequest { Username = "jay", Email = "b@b.com", Password = "pw12345" });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenPasswordCorrect()
    {
        using var db = DbHelper.CreateDb();
        var controller = new AuthController(db, TestConfig());
        await controller.Register(new RegisterRequest { Username = "jay", Email = "a@a.com", Password = "pw12345" });

        var result = await controller.Login(new LoginRequest { Username = "jay", Password = "pw12345" }) as OkObjectResult;

        Assert.NotNull(result);
        var response = result.Value as AuthResponse;
        Assert.False(string.IsNullOrEmpty(response!.Token));
    }

    [Fact]
    public async Task Login_RejectsWrongPassword()
    {
        using var db = DbHelper.CreateDb();
        var controller = new AuthController(db, TestConfig());
        await controller.Register(new RegisterRequest { Username = "jay", Email = "a@a.com", Password = "pw12345" });

        var result = await controller.Login(new LoginRequest { Username = "jay", Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_RejectsUnknownUsername()
    {
        using var db = DbHelper.CreateDb();
        var controller = new AuthController(db, TestConfig());

        var result = await controller.Login(new LoginRequest { Username = "ghost", Password = "pw12345" });

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
