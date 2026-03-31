namespace AgenticCompany.Api.Models;

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserInfo User);
public record UserInfo(Guid Id, string Email, string DisplayName);
