namespace Budgexa.Application.Tests.Budgets.Commands.DeleteBudget;

using System.Net;
using Budgexa.Application.Budgets.Commands.DeleteBudget;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class DeleteBudgetCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        return current;
    }

    [Fact]
    public async Task Handle_UnknownBudget_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new DeleteBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteBudgetCommand(Guid.NewGuid()), CancellationToken.None);

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
        var budget = TestDataSeeder.SeedBudget(db, otherCompanyId, customer.Id);

        var sut = new DeleteBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteBudgetCommand(budget.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingBudget_MarksAsDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var budget = TestDataSeeder.SeedBudget(db, companyId, customer.Id);

        var sut = new DeleteBudgetCommandHandler(db, BuildCurrentUser(companyId));

        await sut.Handle(new DeleteBudgetCommand(budget.Id), CancellationToken.None);

        db.Budgets.Single(b => b.Id == budget.Id).StatusId.Should().Be(StatusIds.Delete);
    }
}
