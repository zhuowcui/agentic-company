using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AgenticCompany.Api.Models;
using AgenticCompany.Core.Entities;
using AgenticCompany.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AgenticCompany.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string DevFallbackKey = "agentic-company-dev-signing-key-min-32-chars!";
    private static readonly PasswordHasher<User> _passwordHasher = new();

    private readonly IUserRepository _users;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IUserRepository users, IConfiguration configuration, IWebHostEnvironment environment)
    {
        _users = users;
        _configuration = configuration;
        _environment = environment;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existing = await _users.GetByEmailAsync(normalizedEmail);
        if (existing is not null)
            return Conflict(new { message = "Email already registered" });

        var user = new User
        {
            Email = normalizedEmail,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _passwordHasher.HashPassword(null!, request.Password),
        };

        user = await _users.CreateAsync(user);

        var token = GenerateToken(user);
        return Ok(new AuthResponse(token, new UserInfo(user.Id, user.Email, user.DisplayName)));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password" });

        var result = _passwordHasher.VerifyHashedPassword(null!, user.PasswordHash, request.Password);
        if (result != PasswordVerificationResult.Success && result != PasswordVerificationResult.SuccessRehashNeeded)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = GenerateToken(user);
        return Ok(new AuthResponse(token, new UserInfo(user.Id, user.Email, user.DisplayName)));
    }

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

    private string GenerateToken(User user)
    {
        var signingKey = _configuration["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            if (_environment.IsDevelopment())
                signingKey = DevFallbackKey;
            else
                throw new InvalidOperationException("Jwt:SigningKey is not configured.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
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
}
