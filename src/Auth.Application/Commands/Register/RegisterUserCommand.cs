using Auth.Application.DTOs;

namespace Auth.Application.Commands.Register;

public record RegisterUserCommand(
    Guid IdempotencyKey,
    string Name,
    string Email,
    string Password,
    string Role);

public interface IRegisterUserCommandHandler
{
    Task<UserResponse> Handle(RegisterUserCommand command, CancellationToken ct);
}
