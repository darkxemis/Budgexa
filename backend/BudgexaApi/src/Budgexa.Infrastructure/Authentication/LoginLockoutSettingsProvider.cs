using Budgexa.Application.Auth;
using Microsoft.Extensions.Configuration;

namespace Budgexa.Infrastructure.Authentication;

public sealed class LoginLockoutSettingsProvider : ILoginLockoutSettingsProvider
{
    public int MaxFailedAttempts { get; }
    public int LockoutMinutes { get; }

    public LoginLockoutSettingsProvider(IConfiguration configuration)
    {
        var section = configuration.GetSection("LoginLockout");
        MaxFailedAttempts = section.GetValue<int>("MaxFailedAttempts");
        LockoutMinutes = section.GetValue<int>("LockoutMinutes");
    }
}