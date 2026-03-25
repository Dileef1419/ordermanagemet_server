using Auth.Application.DTOs;

namespace Auth.Application.Commands.Login;

public record LoginUserCommand(
    string Email,
    string Password);

public interface ILoginUserCommandHandler
{
    Task<LoginResponse> Handle(LoginUserCommand command, CancellationToken ct);
}
