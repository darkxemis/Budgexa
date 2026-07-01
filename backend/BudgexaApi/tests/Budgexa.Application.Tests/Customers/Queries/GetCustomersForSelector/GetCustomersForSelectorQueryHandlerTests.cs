namespace Budgexa.Application.Tests.Customers.Queries.GetCustomersForSelector;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.Queries.GetCustomersForSelector;
using Budgexa.Application.Tests.TestHelpers;
using NSubstitute;

public class GetCustomersForSelectorQueryHandlerTests
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
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Active", taxId: "A1");
        TestDataSeeder.SeedCustomer(db, companyId, deleteStatusId, legalName: "Deleted", taxId: "A2");
        TestDataSeeder.SeedCustomer(db, Guid.NewGuid(), newStatusId, legalName: "Other", taxId: "A3");

        var sut = new GetCustomersForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetCustomersForSelectorQuery(null), CancellationToken.None);

        result.Should().ContainSingle(r => r.Name == "Active");
    }

    [Fact]
    public async Task Handle_AppliesSearchOnLegalTradeAndTaxId()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha", taxId: "A1");
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Beta", tradeName: "BetaBrand", taxId: "B2");

        var sut = new GetCustomersForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var byBrand = await sut.Handle(new GetCustomersForSelectorQuery("betabrand"), CancellationToken.None);
        var byTaxId = await sut.Handle(new GetCustomersForSelectorQuery("A1"), CancellationToken.None);

        byBrand.Should().ContainSingle(r => r.Name == "Beta");
        byTaxId.Should().ContainSingle(r => r.Name == "Alpha");
    }

    [Fact]
    public async Task Handle_ResultsAreOrderedByLegalName()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Zeta", taxId: "Z");
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha", taxId: "A");
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Mu", taxId: "M");

        var sut = new GetCustomersForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetCustomersForSelectorQuery(null), CancellationToken.None);

        result.Select(r => r.Name).Should().ContainInOrder("Alpha", "Mu", "Zeta");
    }
}
