namespace Budgexa.Infrastructure.Tests.Authentication;

using Budgexa.Infrastructure.Authentication;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesDifferentHashesForSamePassword()
    {
        var first = _hasher.Hash("Secret-123!");
        var second = _hasher.Hash("Secret-123!");

        first.Should().NotBe(second);
        first.Should().NotBe("Secret-123!");
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        var hash = _hasher.Hash("Secret-123!");

        _hasher.Verify("Secret-123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var hash = _hasher.Hash("Secret-123!");

        _hasher.Verify("wrong-password", hash).Should().BeFalse();
    }
}
