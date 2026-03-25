using System.Text.Json;
using Auth.Application.Commands.Register;
using Auth.Application.DTOs;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Auth.Infrastructure.CommandHandlers;

public class RegisterUserHandler : IRegisterUserCommandHandler
{
    private readonly AuthDbContext _db;
    private readonly IUserRepository _users;

    public RegisterUserHandler(AuthDbContext db, IUserRepository users)
    {
        _db = db;
        _users = users;
    }

    public async Task<UserResponse> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        // 1. Idempotency check
        var existing = await _db.ProcessedCommands.FindAsync(new object[] { cmd.IdempotencyKey }, ct);
        if (existing is not null && !string.IsNullOrWhiteSpace(existing.ResultPayload))
            return JsonSerializer.Deserialize<UserResponse>(existing.ResultPayload!)!;

        // 2. Business rules
        if (await _users.EmailExistsAsync(cmd.Email, ct))
            throw new InvalidOperationException("Email already exists.");

        // 3. Create user
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password);
        var user = new User
        {
            Name = cmd.Name,
            Email = cmd.Email,
            PasswordHash = passwordHash,
            Role = cmd.Role,
            CreatedAt = DateTime.UtcNow
        };
        await _db.Users.AddAsync(user, ct);

        // 4. Record idempotency
        var result = new UserResponse(user.Id, user.Name, user.Email, user.Role, user.CreatedAt);
        _db.ProcessedCommands.Add(new ProcessedCommand
        {
            IdempotencyKey = cmd.IdempotencyKey,
            CommandType = nameof(RegisterUserCommand),
            ResultPayload = JsonSerializer.Serialize(result),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        });

        // 5. Save
        await _db.SaveChangesAsync(ct);
        return result;
    }
}
