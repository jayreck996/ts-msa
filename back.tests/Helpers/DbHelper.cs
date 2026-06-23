using Microsoft.EntityFrameworkCore;
using QuizApi.Data;

namespace QuizApi.Tests.Helpers;

public static class DbHelper
{
    public static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
