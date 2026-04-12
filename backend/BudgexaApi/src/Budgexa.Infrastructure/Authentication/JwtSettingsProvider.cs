using Budgexa.Application.Auth;
using Microsoft.Extensions.Options;

namespace Budgexa.Infrastructure.Authentication;

public sealed class JwtSettingsProvider : IJwtSettingsProvider
{
    private readonly JwtSettings _jwtSettings;

    public JwtSettingsProvider(IOptions<JwtSettings> options)
    {
        _jwtSettings = options.Value;
    }

    public int RefreshTokenExpirationInDays => _jwtSettings.RefreshTokenExpirationInDays;
}


