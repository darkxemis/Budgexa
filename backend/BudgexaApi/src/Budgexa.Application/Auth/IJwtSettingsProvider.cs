namespace Budgexa.Application.Auth;

public interface IJwtSettingsProvider
{
    int RefreshTokenExpirationInDays { get; }
}

