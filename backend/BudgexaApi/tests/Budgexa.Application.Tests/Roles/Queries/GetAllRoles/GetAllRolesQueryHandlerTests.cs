namespace Budgexa.Application.Tests.Roles.Queries.GetAllRoles;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Roles.Queries.GetAllRoles;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetAllRolesQueryHandlerTests
{
    [Fact]
    public async Task Handle_SuperAdministrator_ReturnsAllRoles()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.Roles.Returns(new[] { RoleNames.SuperAdministrator });

        var sut = new GetAllRolesQueryHandler(db, current);

        var result = await sut.Handle(new GetAllRolesQuery(), CancellationToken.None);

        result.Select(r => r.Id).Should().BeEquivalentTo(new[]
        {
            RoleIds.Freelance,
            RoleIds.Administrator,
            RoleIds.SuperAdministrator,
        });
    }

    [Fact]
    public async Task Handle_NonSuperAdministrator_OnlyReturnsAdminAndFreelance()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.Roles.Returns(new[] { RoleNames.Administrator });

        var sut = new GetAllRolesQueryHandler(db, current);

        var result = await sut.Handle(new GetAllRolesQuery(), CancellationToken.None);

        result.Select(r => r.Id).Should().BeEquivalentTo(new[]
        {
            RoleIds.Freelance,
            RoleIds.Administrator,
        });
    }
}
