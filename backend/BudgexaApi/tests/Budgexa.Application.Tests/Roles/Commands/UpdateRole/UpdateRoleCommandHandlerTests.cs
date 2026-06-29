namespace Budgexa.Application.Tests.Roles.Commands.UpdateRole;

using System.Net;
using Budgexa.Application.Roles.Commands.UpdateRole;
using Budgexa.Application.Roles.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;

public class UpdateRoleCommandHandlerTests
{
    [Fact]
    public async Task Handle_RoleNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var sut = new UpdateRoleCommandHandler(db);

        var act = () => sut.Handle(new UpdateRoleCommand(Guid.NewGuid(), new UpdateRoleDto("any")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Role.NotFound);
    }

    [Fact]
    public async Task Handle_NameTakenByAnotherRole_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var role = Role.Create("editor");
        db.Roles.Add(role);
        db.Roles.Add(Role.Create("manager"));
        await db.SaveChangesAsync();

        var sut = new UpdateRoleCommandHandler(db);

        var act = () => sut.Handle(new UpdateRoleCommand(role.Id, new UpdateRoleDto("manager")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Role.NameExists);
    }

    [Fact]
    public async Task Handle_ValidChange_UpdatesRoleName()
    {
        using var db = TestDbContextFactory.Create();
        var role = Role.Create("editor");
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var sut = new UpdateRoleCommandHandler(db);

        await sut.Handle(new UpdateRoleCommand(role.Id, new UpdateRoleDto("publisher")), CancellationToken.None);

        var updated = db.Roles.Single(r => r.Id == role.Id);
        updated.Name.Should().Be("publisher");
    }

    [Fact]
    public async Task Handle_SameNameForSameRole_IsAllowed()
    {
        using var db = TestDbContextFactory.Create();
        var role = Role.Create("editor");
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var sut = new UpdateRoleCommandHandler(db);

        await sut.Handle(new UpdateRoleCommand(role.Id, new UpdateRoleDto("editor")), CancellationToken.None);

        db.Roles.Single(r => r.Id == role.Id).Name.Should().Be("editor");
    }
}
