namespace Budgexa.Application.Tests.Items.Queries.GetItemsForSelector;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Items.Queries.GetItemsForSelector;
using Budgexa.Application.Tests.TestHelpers;
using NSubstitute;

public class GetItemsForSelectorQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        return current;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyOwnCompanyAndExcludesDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Active", sku: "A1");
        TestDataSeeder.SeedItem(db, companyId, deleteStatusId, name: "Deleted", sku: "A2");
        TestDataSeeder.SeedItem(db, Guid.NewGuid(), newStatusId, name: "Other", sku: "A3");

        var sut = new GetItemsForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetItemsForSelectorQuery(null), CancellationToken.None);

        result.Should().ContainSingle(r => r.Name == "Active");
    }

    [Fact]
    public async Task Handle_AppliesSearchOnNameAndSku()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Alpha", sku: "A1");
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Beta", sku: "B2");

        var sut = new GetItemsForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var byName = await sut.Handle(new GetItemsForSelectorQuery("alpha"), CancellationToken.None);
        var bySku = await sut.Handle(new GetItemsForSelectorQuery("B2"), CancellationToken.None);

        byName.Should().ContainSingle(r => r.Name == "Alpha");
        bySku.Should().ContainSingle(r => r.Name == "Beta");
    }

    [Fact]
    public async Task Handle_ResultsAreOrderedByName()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Zeta", sku: "Z");
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Alpha", sku: "A");
        TestDataSeeder.SeedItem(db, companyId, newStatusId, name: "Mu", sku: "M");

        var sut = new GetItemsForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetItemsForSelectorQuery(null), CancellationToken.None);

        result.Select(r => r.Name).Should().ContainInOrder("Alpha", "Mu", "Zeta");
    }
}
