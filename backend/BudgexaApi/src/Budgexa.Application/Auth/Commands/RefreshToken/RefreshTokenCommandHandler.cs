namespace Budgexa.Application.Auth.Commands.RefreshToken;

using System.Net;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class RefreshTokenCommandHandler(
    IApplicationDbContext db,
    IJwtTokenGenerator jwtTokenGenerator,
    IJwtSettingsProvider jwtSettingsProvider
) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Invalid refresh token.");

        if (!existingToken.IsActive)
        {
            throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Refresh token is expired or revoked.");
        }

        var user = await db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Language)
            .Include(u => u.Status)
            .FirstOrDefaultAsync(u => u.Id == existingToken.UserId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Invalid refresh token.");

        var accessToken = jwtTokenGenerator.GenerateToken(user);
        var newRefreshToken = Budgexa.Domain.Entities.RefreshToken.Create(user.Id, jwtSettingsProvider.RefreshTokenExpirationInDays);

        existingToken.Revoke(newRefreshToken.Token);

        await db.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}",
            accessToken,
            newRefreshToken.Token);
    }
}