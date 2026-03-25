namespace Auth.Application.DTOs;

public record RegisterUserRequest(string Name, string Email, string Password, string Role);
public record LoginRequest(string Email, string Password);

public record UserResponse(Guid UserId, string Name, string Email, string Role, DateTime CreatedAt);
public record LoginResponse(string Token, DateTime ExpiresAt);
