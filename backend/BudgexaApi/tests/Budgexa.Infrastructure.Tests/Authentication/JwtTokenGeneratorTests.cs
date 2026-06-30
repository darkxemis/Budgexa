namespace Budgexa.Infrastructure.Tests.Authentication;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Domain.Entities;
using Budgexa.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

public class JwtTokenGeneratorTests
{
    private static JwtSettings BuildSettings() => new()
    {
        // The HmacSha256 signing key must be at least 128 bits.
        Secret = "this-is-a-test-secret-with-enough-length-1234567890",
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpirationInMinutes = 60,
        RefreshTokenExpirationInDays = 7,
    };

    private static User CreateUser()
    {
        var user = User.Create(
            "alice@example.com",
            "hashed",
            "Alice",
            "Smith",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var role = Role.Create("administrator");
        var ur = UserRole.Create(user.Id, role.Id);
        typeof(UserRole).GetProperty(nameof(UserRole.Role))!
            .SetValue(ur, role);
        user.UserRoles.Add(ur);
        return user;
    }

    [Fact]
    public void GenerateToken_IncludesExpectedClaims()
    {
        var settings = BuildSettings();
        var sut = new JwtTokenGenerator(Options.Create(settings));
        var user = CreateUser();

        var token = sut.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be(settings.Issuer);
        jwt.Audiences.Should().ContainSingle(a => a == settings.Audience);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == user.FirstName);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.FamilyName && c.Value == user.LastName);
        jwt.Claims.Should().Contain(c => c.Type == "company_id" && c.Value == user.CompanyId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "administrator");
    }

    [Fact]
    public void GenerateToken_SetsExpirationFromSettings()
    {
        var settings = BuildSettings();
        var sut = new JwtTokenGenerator(Options.Create(settings));
        var user = CreateUser();

        var before = DateTime.UtcNow;
        var token = sut.GenerateToken(user);
        var after = DateTime.UtcNow;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidTo.Should().BeOnOrAfter(before.AddMinutes(settings.ExpirationInMinutes).AddSeconds(-2));
        jwt.ValidTo.Should().BeOnOrBefore(after.AddMinutes(settings.ExpirationInMinutes).AddSeconds(2));
    }
}
