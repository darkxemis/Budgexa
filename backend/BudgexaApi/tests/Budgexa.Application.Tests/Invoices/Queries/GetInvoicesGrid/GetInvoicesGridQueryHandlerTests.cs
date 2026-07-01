namespace Budgexa.Application.Tests.Invoices.Queries.GetInvoicesGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Queries.GetInvoicesGrid;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetInvoicesGridQueryHandlerTests
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
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        TestDataSeeder.SeedInvoice(db, companyId, customer.Id, number: "INV-001");
        TestDataSeeder.SeedInvoice(db, companyId, customer.Id, statusId: StatusIds.Delete, number: "INV-002");

        var otherCompanyId = Guid.NewGuid();
        var otherCustomer = TestDataSeeder.SeedCustomer(db, otherCompanyId, newStatusId);
        TestDataSeeder.SeedInvoice(db, otherCompanyId, otherCustomer.Id, number: "INV-OTHER");

        var sut = new GetInvoicesGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetInvoicesGridQuery(new GridRequestDto(1, 10, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Data.Should().ContainSingle(i => i.Number == "INV-001");
    }

    [Fact]
    public async Task Handle_AppliesSearchAcrossKeyColumns()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var alpha = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha");
        var beta = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Beta", taxId: "B2");
        TestDataSeeder.SeedInvoice(db, companyId, alpha.Id, number: "INV-001");
        TestDataSeeder.SeedInvoice(db, companyId, beta.Id, number: "INV-002");

        var sut = new GetInvoicesGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetInvoicesGridQuery(new GridRequestDto(1, 10, null, null, "beta")),
            CancellationToken.None);

        response.Data.Should().ContainSingle(i => i.Number == "INV-002");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSlice()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        for (var i = 0; i < 5; i++)
        {
            TestDataSeeder.SeedInvoice(db, companyId, customer.Id, number: $"INV-{i:D3}");
        }

        var sut = new GetInvoicesGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetInvoicesGridQuery(new GridRequestDto(2, 2, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(5);
        response.TotalPages.Should().Be(3);
        response.Data.Should().HaveCount(2);
    }
}
