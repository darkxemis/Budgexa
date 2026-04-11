namespace Budgexa.Domain.Interfaces;

using Budgexa.Domain.Entities;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
