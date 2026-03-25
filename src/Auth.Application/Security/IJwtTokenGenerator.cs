namespace Auth.Application.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, string role, DateTime nowUtc, out DateTime expiresUtc);
}
