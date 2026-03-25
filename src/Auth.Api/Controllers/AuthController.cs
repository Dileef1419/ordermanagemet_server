using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Auth.Application.Commands.Register;
using Auth.Application.Commands.Login;
using Auth.Application.DTOs;

namespace Auth.Api.Controllers;

/// <summary>
/// Auth Controller — Register (Command via EF Core) and Login (JWT issuance).
/// Follows CQRS pattern: POST = Commands.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    /// <summary>Register a new user (idempotent — requires Idempotency-Key header).</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromServices] IRegisterUserCommandHandler handler,
        [FromServices] IValidator<RegisterUserCommand> validator,
        CancellationToken ct)
    {
        if (idempotencyKey == Guid.Empty)
            return BadRequest("Idempotency-Key header is required (GUID).");

        var command = new RegisterUserCommand(
            idempotencyKey,
            request.Name,
            request.Email,
            request.Password,
            request.Role);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        var result = await handler.Handle(command, ct);
        return Created(string.Empty, result);
    }

    /// <summary>Login and get JWT token.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        [FromServices] ILoginUserCommandHandler handler,
        [FromServices] IValidator<LoginUserCommand> validator,
        CancellationToken ct)
    {
        var command = new LoginUserCommand(request.Email, request.Password);

        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
            return BadRequest(new ValidationProblemDetails(
                validation.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())));

        try
        {
            var result = await handler.Handle(command, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}
