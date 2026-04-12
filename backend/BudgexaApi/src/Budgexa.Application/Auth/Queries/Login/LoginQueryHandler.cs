namespace Budgexa.Application.Auth.Queries.Login;

using MediatR;
using System.Net;
using Budgexa.Application.Auth.DTOs;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using Budgexa.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

public sealed class LoginQueryHandler : IRequestHandler<LoginQuery, AuthResult>
{
    private readonly IUserRepository userRepository;
    private readonly IRefreshTokenRepository refreshTokenRepository;
    private readonly IJwtTokenGenerator jwtTokenGenerator;
    private readonly IPasswordHasher passwordHasher;
    private readonly IUnitOfWork unitOfWork;
    private readonly JwtSettings jwtSettings;

    public LoginQueryHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtOptions)
    {
        this.userRepository = userRepository;
        this.refreshTokenRepository = refreshTokenRepository;
        this.jwtTokenGenerator = jwtTokenGenerator;
        this.passwordHasher = passwordHasher;
        this.unitOfWork = unitOfWork;
        this.jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidCredentials, "Invalid email or password.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException(HttpStatusCode.Unauthorized, ErrorTags.Auth.InvalidCredentials, "Invalid email or password.");
        }

        var accessToken = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = RefreshToken.Create(user.Id, jwtSettings.RefreshTokenExpirationInDays);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResult(user.Id, accessToken, refreshToken.Token, user.Email, $"{user.FirstName} {user.LastName}");
    }
}
