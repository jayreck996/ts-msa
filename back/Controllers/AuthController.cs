using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuizApi.Data;
using QuizApi.Models;

namespace QuizApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController(AppDbContext db, IConfiguration config) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and password are required");

        if (await db.Users.AnyAsync(u => u.Username == request.Username))
            return Conflict("Username already taken");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return Ok(BuildAuthResponse(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password");

        return Ok(BuildAuthResponse(user));
    }

    private AuthResponse BuildAuthResponse(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            ],
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.Id,
            Username = user.Username,
        };
    }
}
