namespace Budgexa.Application.Auth.Commands.RefreshToken;

using System.Net;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IUnitOfWork unitOfWork,
    IJwtSettingsProvider jwtSettingsProvider
) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Invalid refresh token.");

        if (!existingToken.IsActive)
        {
            throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Refresh token is expired or revoked.");
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Invalid refresh token.");

        var accessToken = jwtTokenGenerator.GenerateToken(user);
        var newRefreshToken = Budgexa.Domain.Entities.RefreshToken.Create(user.Id, jwtSettingsProvider.RefreshTokenExpirationInDays);

        existingToken.Revoke(newRefreshToken.Token);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}",
            accessToken,
            newRefreshToken.Token);
    }
}