namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class RoleTests
{
    [Fact]
    public void Create_WithoutExplicitId_GeneratesNewGuid()
    {
        var role = Role.Create("admin");

        role.Id.Should().NotBe(Guid.Empty);
        role.Name.Should().Be("admin");
        role.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithExplicitId_UsesProvidedId()
    {
        var id = Guid.NewGuid();

        var role = Role.Create("admin", id);

        role.Id.Should().Be(id);
    }

    [Fact]
    public void Update_ChangesNameOnly()
    {
        var id = Guid.NewGuid();
        var role = Role.Create("admin", id);

        role.Update("super-admin");

        role.Name.Should().Be("super-admin");
        role.Id.Should().Be(id);
    }
}
