using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private const string SigningKey = "agentic-company-dev-signing-key-min-32-chars!";
    private const string Salt = "agentic-company-static-salt";

    public AuthController(IUserRepository users) => _users = users;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var existing = await _users.GetByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict(new { message = "Email already registered" });

        var user = new User
        {
            Email = request.Email.Trim().ToLowerInvariant(),
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = HashPassword(request.Password),
        };

        user = await _users.CreateAsync(user);

        var token = GenerateToken(user);
        return Ok(new AuthResponse(token, new UserInfo(user.Id, user.Email, user.DisplayName)));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || user.PasswordHash != HashPassword(request.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = GenerateToken(user);
        return Ok(new AuthResponse(token, new UserInfo(user.Id, user.Email, user.DisplayName)));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserInfo>> Me()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim is null || !Guid.TryParse(idClaim, out var userId))
            return Unauthorized();

        var user = await _users.GetByIdAsync(userId);
        if (user is null) return NotFound();

        return Ok(new UserInfo(user.Id, user.Email, user.DisplayName));
    }

    private static string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
        };

        var token = new JwtSecurityToken(
            issuer: "agentic-company",
            audience: "agentic-company",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        var keyBytes = Encoding.UTF8.GetBytes(Salt);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hash);
    }
}
