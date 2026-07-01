namespace Budgexa.Application.Tests.Budgets.Queries.GetBudgetById;

using System.Net;
using Budgexa.Application.Budgets.Queries.GetBudgetById;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class GetBudgetByIdQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    [Fact]
    public async Task Handle_UnknownBudget_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new GetBudgetByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetBudgetByIdQuery(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Budget.NotFound);
    }

    [Fact]
    public async Task Handle_BudgetFromOtherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var otherCompanyId = Guid.NewGuid();
        var customer = TestDataSeeder.SeedCustomer(db, otherCompanyId, newStatusId);
        var foreign = TestDataSeeder.SeedBudget(db, otherCompanyId, customer.Id);

        var sut = new GetBudgetByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetBudgetByIdQuery(foreign.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingBudget_ReturnsDtoWithLines()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Acme");
        var budget = TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: "BUD-0001");

        var sut = new GetBudgetByIdQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetBudgetByIdQuery(budget.Id), CancellationToken.None);

        result.Id.Should().Be(budget.Id);
        result.Number.Should().Be("BUD-0001");
        result.CustomerName.Should().Be("Acme");
        result.Lines.Should().ContainSingle();
    }
}
