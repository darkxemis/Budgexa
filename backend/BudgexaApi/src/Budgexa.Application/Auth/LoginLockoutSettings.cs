namespace Budgexa.Application.Auth;

public interface ILoginLockoutSettingsProvider
{
    int MaxFailedAttempts { get; }
    int LockoutMinutes { get; }
}

