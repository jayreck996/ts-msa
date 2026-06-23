using Microsoft.EntityFrameworkCore;
using QuizApi.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=quiz.db"));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
}

app.UseCors();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

app.Run();
