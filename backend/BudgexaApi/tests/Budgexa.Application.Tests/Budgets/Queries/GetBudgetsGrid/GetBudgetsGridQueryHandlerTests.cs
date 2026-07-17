namespace Budgexa.Application.Tests.Budgets.Queries.GetBudgetsGrid;

using Budgexa.Application.Budgets.Queries.GetBudgetsGrid;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetBudgetsGridQueryHandlerTests
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
        TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: "BUD-001");
        TestDataSeeder.SeedBudget(db, companyId, customer.Id, statusId: StatusIds.Delete, number: "BUD-002");

        var otherCompanyId = Guid.NewGuid();
        var otherCustomer = TestDataSeeder.SeedCustomer(db, otherCompanyId, newStatusId);
        TestDataSeeder.SeedBudget(db, otherCompanyId, otherCustomer.Id, number: "BUD-OTHER");

        var sut = new GetBudgetsGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetBudgetsGridQuery(new GridRequestDto(1, 10, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Data.Should().ContainSingle(b => b.Number == "BUD-001");
    }

    [Fact]
    public async Task Handle_AppliesSearchAcrossKeyColumns()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customerA = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha", taxId: "A1");
        var customerB = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Beta", taxId: "B2");
        TestDataSeeder.SeedBudget(db, companyId, customerA.Id, number: "BUD-001");
        TestDataSeeder.SeedBudget(db, companyId, customerB.Id, number: "BUD-002");

        var sut = new GetBudgetsGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetBudgetsGridQuery(new GridRequestDto(1, 10, null, null, "beta")),
            CancellationToken.None);

        response.Data.Should().ContainSingle(b => b.Number == "BUD-002");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSlice()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        for (var i = 0; i < 5; i++)
        {
            TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: $"BUD-{i:D3}");
        }

        var sut = new GetBudgetsGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetBudgetsGridQuery(new GridRequestDto(2, 2, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(5);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(2);
        response.TotalPages.Should().Be(3);
        response.Data.Should().HaveCount(2);
    }
}
