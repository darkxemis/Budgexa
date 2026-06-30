namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class StatusTests
{
    [Fact]
    public void Create_WithoutExplicitId_GeneratesNewGuidAndInitializesProperties()
    {
        var status = Status.Create("User", "New", 1);

        status.Id.Should().NotBe(Guid.Empty);
        status.Group.Should().Be("User");
        status.Name.Should().Be("New");
        status.Value.Should().Be(1);
        status.Translations.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var status = Status.Create("Company", "Active", 1, id);

        status.Id.Should().Be(id);
    }
}
