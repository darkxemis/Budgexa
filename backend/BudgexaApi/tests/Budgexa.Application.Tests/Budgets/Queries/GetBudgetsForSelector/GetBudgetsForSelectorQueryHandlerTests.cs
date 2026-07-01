namespace Budgexa.Application.Tests.Budgets.Queries.GetBudgetsForSelector;

using Budgexa.Application.Budgets.Queries.GetBudgetsForSelector;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetBudgetsForSelectorQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        return current;
    }

    [Fact]
    public async Task Handle_ScopesByCompanyAndExcludesDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: "BUD-001");
        TestDataSeeder.SeedBudget(db, companyId, customer.Id, statusId: StatusIds.Delete, number: "BUD-002");

        var otherCompanyId = Guid.NewGuid();
        var otherCustomer = TestDataSeeder.SeedCustomer(db, otherCompanyId, newStatusId);
        TestDataSeeder.SeedBudget(db, otherCompanyId, otherCustomer.Id, number: "BUD-OTHER");

        var sut = new GetBudgetsForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetBudgetsForSelectorQuery(null), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("BUD-001");
    }

    [Fact]
    public async Task Handle_SearchMatchesNumberOrCustomerLegalName()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var alpha = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha", taxId: "A1");
        var beta = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Beta", taxId: "B2");
        TestDataSeeder.SeedBudget(db, companyId, alpha.Id, number: "BUD-001");
        TestDataSeeder.SeedBudget(db, companyId, beta.Id, number: "BUD-002");

        var sut = new GetBudgetsForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetBudgetsForSelectorQuery("beta"), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Be("BUD-002");
    }
}
