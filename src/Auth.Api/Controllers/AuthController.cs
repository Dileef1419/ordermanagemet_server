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

    /// <summary>Get all registered users (Admin only).</summary>
    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromServices] Auth.Domain.Repositories.IUserRepository repository,
        CancellationToken ct)
    {
        var users = await repository.GetAllAsync(ct);
        var response = users.Select(u => new UserResponse(u.Id, u.Name, u.Email, u.Role, u.CreatedAt));
        return Ok(response);
    }

    /// <summary>Get user profile by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(
        Guid id,
        [FromServices] Auth.Domain.Repositories.IUserRepository repository,
        CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(id, ct);
        if (user == null) return NotFound();

        return Ok(new UserResponse(user.Id, user.Name, user.Email, user.Role, user.CreatedAt));
    }

    /// <summary>Delete a user by email (Admin only).</summary>
    [HttpDelete("users/{email}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(
        string email,
        [FromServices] Auth.Domain.Repositories.IUserRepository repository,
        CancellationToken ct)
    {
        var success = await repository.DeleteAsync(email, ct);
        return success ? NoContent() : NotFound();
    }
}
