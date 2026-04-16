namespace Budgexa.Application.Auth.Queries.Login;

using MediatR;
using System.Net;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;

public sealed class LoginQueryHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IJwtSettingsProvider jwtSettingsProvider,
    ILoginLockoutSettingsProvider lockoutSettingsProvider
) : IRequestHandler<LoginQuery, AuthResult>
{
    public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidCredentials, "Invalid email or password.");

        if (user.IsLockedOut())
        {
            throw new AppException(
                HttpStatusCode.Forbidden,
                ErrorTags.Auth.AccountLocked,
                $"Account is locked until {user.LockoutEnd:HH:mm:ss} UTC.",
                new Dictionary<string, string>
                {
                    { "unlockAtUtc", user.LockoutEnd?.ToString("o") ?? string.Empty }
                }
            );
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RegisterFailedLogin(
                lockoutSettingsProvider.MaxFailedAttempts,
                TimeSpan.FromMinutes(lockoutSettingsProvider.LockoutMinutes)
            );
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidCredentials, "Invalid email or password.");
        }

        user.ResetLoginFailures();

        var accessToken = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = RefreshToken.Create(user.Id, jwtSettingsProvider.RefreshTokenExpirationInDays);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(user.Id, accessToken, refreshToken.Token, user.Email, $"{user.FirstName} {user.LastName}");
    }
}