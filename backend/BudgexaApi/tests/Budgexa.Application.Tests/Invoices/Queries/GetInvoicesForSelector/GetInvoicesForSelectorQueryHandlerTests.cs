namespace Budgexa.Application.Tests.Invoices.Queries.GetInvoicesForSelector;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Queries.GetInvoicesForSelector;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetInvoicesForSelectorQueryHandlerTests
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
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Acme");
        TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-001");
        TestDataSeeder.SeedInvoice(db, companyId, customer.Id, statusId: StatusIds.Delete, series: "A", number: "INV-002");

        var otherCompanyId = Guid.NewGuid();
        var otherCustomer = TestDataSeeder.SeedCustomer(db, otherCompanyId, newStatusId);
        TestDataSeeder.SeedInvoice(db, otherCompanyId, otherCustomer.Id, series: "X", number: "INV-OTHER");

        var sut = new GetInvoicesForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetInvoicesForSelectorQuery(null), CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().Name.Should().Contain("INV-001").And.Contain("Acme");
    }

    [Fact]
    public async Task Handle_SearchMatchesNumberOrSeriesOrCustomer()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var alpha = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha");
        var beta = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Beta", taxId: "B2");
        TestDataSeeder.SeedInvoice(db, companyId, alpha.Id, series: "A", number: "INV-001");
        TestDataSeeder.SeedInvoice(db, companyId, beta.Id, series: "B", number: "INV-002");

        var sut = new GetInvoicesForSelectorQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetInvoicesForSelectorQuery("beta"), CancellationToken.None);

        result.Should().ContainSingle().Which.Name.Should().Contain("INV-002");
    }
}
