namespace Budgexa.Application.Tests.Roles.Commands.CreateRole;

using System.Net;
using Budgexa.Application.Roles.Commands.CreateRole;
using Budgexa.Application.Roles.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;

public class CreateRoleCommandHandlerTests
{
    [Fact]
    public async Task Handle_NewName_PersistsRole()
    {
        using var db = TestDbContextFactory.Create();

        var sut = new CreateRoleCommandHandler(db);

        var id = await sut.Handle(new CreateRoleCommand(new RoleCreateDto("manager")), CancellationToken.None);

        db.Roles.Should().ContainSingle(r => r.Id == id && r.Name == "manager");
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        db.Roles.Add(Role.Create("manager"));
        await db.SaveChangesAsync();

        var sut = new CreateRoleCommandHandler(db);

        var act = () => sut.Handle(new CreateRoleCommand(new RoleCreateDto("manager")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Role.NameExists);
    }
}
