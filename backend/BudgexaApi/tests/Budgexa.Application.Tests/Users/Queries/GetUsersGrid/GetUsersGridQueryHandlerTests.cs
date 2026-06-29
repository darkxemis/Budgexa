namespace Budgexa.Application.Tests.Users.Queries.GetUsersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Queries.GetUsersGrid;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetUsersGridQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId, Guid languageId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(languageId);
        return current;
    }

    [Fact]
    public async Task Handle_ScopesByCompanyAndExcludesDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);

        TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "alice@example.com");
        TestDataSeeder.SeedUser(db, companyId, languageId, deleteStatusId, email: "deleted@example.com");
        TestDataSeeder.SeedUser(db, Guid.NewGuid(), languageId, newStatusId, email: "other@example.com");

        var current = BuildCurrentUser(companyId, LanguageIds.English);
        var sut = new GetUsersGridQueryHandler(db, current);

        var response = await sut.Handle(new GetUsersGridQuery(new GridRequestDto(1, 10, null, null, null)), CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Data.Should().ContainSingle(u => u.Email == "alice@example.com");
    }

    [Fact]
    public async Task Handle_AppliesSearchAcrossKeyColumns()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);

        TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "alice@example.com", firstName: "Alice", lastName: "Anderson");
        TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "bob@example.com", firstName: "Bob", lastName: "Brown");

        var current = BuildCurrentUser(companyId, LanguageIds.English);
        var sut = new GetUsersGridQueryHandler(db, current);

        var response = await sut.Handle(new GetUsersGridQuery(new GridRequestDto(1, 10, null, null, "bob")), CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Data.Should().ContainSingle(u => u.Email == "bob@example.com");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSlice()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);

        for (var i = 0; i < 5; i++)
        {
            TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: $"user{i}@example.com");
        }

        var current = BuildCurrentUser(companyId, LanguageIds.English);
        var sut = new GetUsersGridQueryHandler(db, current);

        var response = await sut.Handle(new GetUsersGridQuery(new GridRequestDto(2, 2, null, null, null)), CancellationToken.None);

        response.TotalCount.Should().Be(5);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(2);
        response.TotalPages.Should().Be(3);
        response.Data.Should().HaveCount(2);
    }
}
