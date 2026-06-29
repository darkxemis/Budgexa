namespace Budgexa.Application.Auth.Queries.Login;

using MediatR;
using System.Net;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class LoginQueryHandler(
    IApplicationDbContext db,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher,
    IJwtSettingsProvider jwtSettingsProvider,
    ILoginLockoutSettingsProvider lockoutSettingsProvider
) : IRequestHandler<LoginQuery, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Language)
            .Include(u => u.Status)
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken)
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
            await db.SaveChangesAsync(cancellationToken);
            throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidCredentials, "Invalid email or password.");
        }

        if (user.Company != null && !user.Company.IsContractValid())
        {
            throw new AppException(
                HttpStatusCode.Forbidden,
                ErrorTags.Auth.ContractExpired,
                "Your contract has expired. Please contact your administrator."
            );
        }

        user.ResetLoginFailures();

        var accessToken = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = RefreshToken.Create(user.Id, jwtSettingsProvider.RefreshTokenExpirationInDays);

        await db.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}",
            accessToken,
            refreshToken.Token);
    }
}