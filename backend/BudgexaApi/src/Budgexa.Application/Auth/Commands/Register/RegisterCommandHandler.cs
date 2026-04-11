namespace Budgexa.Application.Auth.Commands.Register;

using MediatR;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Interfaces;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, hash, request.FirstName, request.LastName);

        await userRepository.AddAsync(user, cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(user);

        return new AuthResult(token, user.Email, $"{user.FirstName} {user.LastName}");
    }
}
