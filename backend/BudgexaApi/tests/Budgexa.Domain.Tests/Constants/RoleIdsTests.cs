namespace Budgexa.Domain.Tests.Constants;

using Budgexa.Domain.Constants;

public class RoleIdsTests
{
    [Fact]
    public void RoleGuids_AreStableAndUnique()
    {
        var ids = new[]
        {
            RoleIds.Freelance,
            RoleIds.Administrator,
            RoleIds.SuperAdministrator
        };

        ids.Should().OnlyHaveUniqueItems();
        ids.Should().NotContain(Guid.Empty);
    }

    [Fact]
    public void FreelanceId_MatchesKnownValue()
    {
        RoleIds.Freelance.Should().Be(Guid.Parse("9f3c2a6e-8b71-4b8d-9c2a-5f6e3d1a7c90"));
    }
}
