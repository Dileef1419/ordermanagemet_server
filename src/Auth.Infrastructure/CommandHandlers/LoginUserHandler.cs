using Auth.Application.Commands.Login;
using Auth.Application.DTOs;
using Auth.Application.Security;
using Auth.Domain.Repositories;
using BCrypt.Net;

namespace Auth.Infrastructure.CommandHandlers;

public class LoginUserHandler : ILoginUserCommandHandler
{
    private readonly IUserRepository _users;
    private readonly IJwtTokenGenerator _jwt;

    public LoginUserHandler(IUserRepository users, IJwtTokenGenerator jwt)
    {
        _users = users;
        _jwt = jwt;
    }

    public async Task<LoginResponse> Handle(LoginUserCommand command, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(command.Email, ct);
        if (user is null) throw new UnauthorizedAccessException("Invalid credentials.");

        var valid = BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash);
        if (!valid) throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwt.GenerateToken(user.Id, user.Name, user.Email, user.Role, DateTime.UtcNow, out var expiresAt);
        return new LoginResponse(token, expiresAt);
    }
}
