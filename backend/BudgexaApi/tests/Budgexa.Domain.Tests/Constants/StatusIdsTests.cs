namespace Budgexa.Domain.Tests.Constants;

using Budgexa.Domain.Constants;

public class StatusIdsTests
{
    [Fact]
    public void StringGuids_MatchParsedGuids()
    {
        StatusIds.New.Should().Be(Guid.Parse(StatusIds.NewString));
        StatusIds.Delete.Should().Be(Guid.Parse(StatusIds.DeleteString));
    }

    [Fact]
    public void StatusGuids_AreUnique()
    {
        StatusIds.New.Should().NotBe(StatusIds.Delete);
        StatusIds.New.Should().NotBe(Guid.Empty);
        StatusIds.Delete.Should().NotBe(Guid.Empty);
    }
}
