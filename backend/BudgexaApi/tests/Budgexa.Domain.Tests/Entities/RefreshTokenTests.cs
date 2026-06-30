namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Create_GeneratesUniqueTokenAndFutureExpiration()
    {
        var userId = Guid.NewGuid();

        var token = RefreshToken.Create(userId, expirationInDays: 7);

        token.Id.Should().NotBe(Guid.Empty);
        token.UserId.Should().Be(userId);
        token.Token.Should().NotBeNullOrWhiteSpace();
        token.RevokedAt.Should().BeNull();
        token.ReplacedByToken.Should().BeNull();
        token.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
        token.IsExpired.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_TwoTokens_AreUnique()
    {
        var first = RefreshToken.Create(Guid.NewGuid(), 1);
        var second = RefreshToken.Create(Guid.NewGuid(), 1);

        first.Token.Should().NotBe(second.Token);
    }

    [Fact]
    public void Revoke_WithoutReplacement_SetsRevokedAtAndLeavesReplacementNull()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), 1);

        token.Revoke();

        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        token.ReplacedByToken.Should().BeNull();
        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_WithReplacement_StoresReplacedByToken()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), 1);

        token.Revoke("new-token");

        token.ReplacedByToken.Should().Be("new-token");
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpirationIsZeroDays_ReturnsTrue()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), 0);

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }
}
