namespace Budgexa.Infrastructure.Tests.Authentication;

using Budgexa.Application.Auth;
using Budgexa.Infrastructure.Authentication;
using Microsoft.Extensions.Configuration;

public class LoginLockoutSettingsProviderTests
{
    [Fact]
    public void Properties_ReflectConfigurationValues()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "LoginLockout:MaxFailedAttempts", "7" },
                { "LoginLockout:LockoutMinutes", "20" },
            })
            .Build();

        ILoginLockoutSettingsProvider sut = new LoginLockoutSettingsProvider(configuration);

        sut.MaxFailedAttempts.Should().Be(7);
        sut.LockoutMinutes.Should().Be(20);
    }

    [Fact]
    public void Properties_MissingValues_ReturnZero()
    {
        var configuration = new ConfigurationBuilder().Build();

        var sut = new LoginLockoutSettingsProvider(configuration);

        sut.MaxFailedAttempts.Should().Be(0);
        sut.LockoutMinutes.Should().Be(0);
    }
}
