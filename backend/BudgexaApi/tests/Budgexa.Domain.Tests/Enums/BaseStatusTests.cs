namespace Budgexa.Domain.Tests.Enums;

using Budgexa.Domain.Enums;

public class BaseStatusTests
{
    [Fact]
    public void EnumValues_MatchExpectedIntegers()
    {
        ((int)BaseStatus.New).Should().Be(1);
        ((int)BaseStatus.Delete).Should().Be(2);
    }
}
