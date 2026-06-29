namespace Budgexa.Domain.Tests.Constants;

using Budgexa.Domain.Constants;

public class RoleNamesTests
{
    [Fact]
    public void RoleNames_AreLowercaseAndDistinct()
    {
        var names = new[]
        {
            RoleNames.Freelance,
            RoleNames.Administrator,
            RoleNames.SuperAdministrator
        };

        names.Should().OnlyHaveUniqueItems();
        names.Should().OnlyContain(n => n == n.ToLowerInvariant());
    }
}
