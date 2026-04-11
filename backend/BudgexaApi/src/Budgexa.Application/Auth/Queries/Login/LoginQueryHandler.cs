namespace Budgexa.Application.Auth.Queries.Login;

using MediatR;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Domain.Interfaces;

public sealed class LoginQueryHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher) : IRequestHandler<LoginQuery, AuthResult>
{
    public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return new AuthResult(token, user.Email, $"{user.FirstName} {user.LastName}");
    }
}
