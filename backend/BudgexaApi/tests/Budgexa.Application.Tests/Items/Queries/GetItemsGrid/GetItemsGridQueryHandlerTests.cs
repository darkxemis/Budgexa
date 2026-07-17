namespace Budgexa.Application.Tests.Items.Queries.GetItemsGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.Queries.GetItemsGrid;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetItemsGridQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    [Fact]
    public async Task Handle_ScopesByCompanyAndExcludesDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Alpha", sku: "A");
        TestDataSeeder.SeedItem(db, companyId, deleteStatusId, name: "Deleted", sku: "D");
        TestDataSeeder.SeedItem(db, Guid.NewGuid(), newStatusId, name: "Other", sku: "O");

        var sut = new GetItemsGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetItemsGridQuery(new GridRequestDto(1, 10, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Data.Should().ContainSingle(i => i.Name == "Alpha");
    }

    [Fact]
    public async Task Handle_AppliesSearch()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Hourly Consulting", sku: "CONS");
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Software License", sku: "LIC");

        var sut = new GetItemsGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetItemsGridQuery(new GridRequestDto(1, 10, null, null, "consulting")),
            CancellationToken.None);

        response.Data.Should().ContainSingle(i => i.Name == "Hourly Consulting");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSlice()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        for (var i = 0; i < 5; i++)
        {
            TestDataSeeder.SeedItem(db, companyId, newStatusId, name: $"Item {i:D2}", sku: $"S-{i:D2}");
        }

        var sut = new GetItemsGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetItemsGridQuery(new GridRequestDto(2, 2, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(5);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(2);
        response.TotalPages.Should().Be(3);
        response.Data.Should().HaveCount(2);
    }
}
