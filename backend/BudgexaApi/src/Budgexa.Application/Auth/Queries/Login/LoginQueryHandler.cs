namespace Budgexa.Application.Auth.Queries.Login;

using MediatR;
using System.Net;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;

public sealed class LoginQueryHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher) : IRequestHandler<LoginQuery, AuthResult>
{
    public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidCredentials, "Invalid email or password.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidCredentials, "Invalid email or password.");
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return new AuthResult(user.Id, token, user.Email, $"{user.FirstName} {user.LastName}");
    }
}
