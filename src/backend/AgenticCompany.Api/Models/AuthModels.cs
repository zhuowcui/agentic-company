using System.ComponentModel.DataAnnotations;

namespace AgenticCompany.Api.Models;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password,
    [Required, MaxLength(200)] string DisplayName);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);
public record AuthResponse(string Token, UserInfo User);
public record UserInfo(Guid Id, string Email, string DisplayName);
