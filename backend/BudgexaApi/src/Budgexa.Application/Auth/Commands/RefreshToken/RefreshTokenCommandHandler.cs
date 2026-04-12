namespace Budgexa.Application.Auth.Commands.RefreshToken;

using System.Net;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using Budgexa.Infrastructure.Authentication;
using MediatR;
using Microsoft.Extensions.Options;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IRefreshTokenRepository refreshTokenRepository;
    private readonly IUserRepository userRepository;
    private readonly IJwtTokenGenerator jwtTokenGenerator;
    private readonly IUnitOfWork unitOfWork;
    private readonly JwtSettings jwtSettings;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions)
    {
        this.refreshTokenRepository = refreshTokenRepository;
        this.userRepository = userRepository;
        this.jwtTokenGenerator = jwtTokenGenerator;
        this.unitOfWork = unitOfWork;
        this.jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(request.Token, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Invalid refresh token.");

        if (!existingToken.IsActive)
        {
            throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "Refresh token is expired or revoked.");
        }

        var user = await userRepository.GetByIdAsync(existingToken.UserId, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidRefreshToken, "User not found.");

        var accessToken = jwtTokenGenerator.GenerateToken(user);
        var newRefreshToken = Domain.Entities.RefreshToken.Create(user.Id, jwtSettings.RefreshTokenExpirationInDays);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(user.Id, accessToken, newRefreshToken.Token, user.Email, $"{user.FirstName} {user.LastName}");
    }
}
