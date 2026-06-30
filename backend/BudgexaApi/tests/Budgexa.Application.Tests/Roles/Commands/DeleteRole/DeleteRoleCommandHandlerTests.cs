namespace Budgexa.Application.Tests.Roles.Commands.DeleteRole;

using System.Net;
using Budgexa.Application.Roles.Commands.DeleteRole;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;

public class DeleteRoleCommandHandlerTests
{
    [Fact]
    public async Task Handle_RoleNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var sut = new DeleteRoleCommandHandler(db);

        var act = () => sut.Handle(new DeleteRoleCommand(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Role.NotFound);
    }

    [Fact]
    public async Task Handle_ExistingRole_RemovesIt()
    {
        using var db = TestDbContextFactory.Create();
        var role = Role.Create("temp");
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var sut = new DeleteRoleCommandHandler(db);

        await sut.Handle(new DeleteRoleCommand(role.Id), CancellationToken.None);

        db.Roles.Should().BeEmpty();
    }
}
