namespace Budgexa.Infrastructure.Tests.Authentication;

using Budgexa.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

public class JwtSettingsProviderTests
{
    [Fact]
    public void RefreshTokenExpirationInDays_ReflectsOptionsValue()
    {
        var settings = new JwtSettings
        {
            Secret = "test-secret",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 14,
        };

        var sut = new JwtSettingsProvider(Options.Create(settings));

        sut.RefreshTokenExpirationInDays.Should().Be(14);
    }
}
