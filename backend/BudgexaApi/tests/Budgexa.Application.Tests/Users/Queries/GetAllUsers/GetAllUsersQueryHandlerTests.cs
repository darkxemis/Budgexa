namespace Budgexa.Application.Tests.Users.Queries.GetAllUsers;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Queries.GetAllUsers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetAllUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_FiltersByCompanyAndExcludesDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);

        TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "active@example.com");
        TestDataSeeder.SeedUser(db, companyId, languageId, deleteStatusId, email: "deleted@example.com");
        TestDataSeeder.SeedUser(db, Guid.NewGuid(), languageId, newStatusId, email: "othercompany@example.com");

        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);

        var sut = new GetAllUsersQueryHandler(db, current);

        var result = await sut.Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Select(u => u.Email).Should().ContainSingle(e => e == "active@example.com");
    }

    [Fact]
    public async Task Handle_NoUsers_ReturnsEmpty()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);

        var sut = new GetAllUsersQueryHandler(db, current);

        var result = await sut.Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
