namespace Budgexa.Application.Tests.Budgets.Commands.ChangeBudgetStatus;

using System.Net;
using Budgexa.Application.Budgets.Commands.ChangeBudgetStatus;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class ChangeBudgetStatusCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    [Fact]
    public async Task Handle_UnknownBudget_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new ChangeBudgetStatusCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new ChangeBudgetStatusCommand(Guid.NewGuid(), new ChangeBudgetStatusDto(StatusIds.Budget.Sent)),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Budget.NotFound);
    }

    [Fact]
    public async Task Handle_UnknownTargetStatus_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var budget = TestDataSeeder.SeedBudget(db, companyId, customer.Id);

        var sut = new ChangeBudgetStatusCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new ChangeBudgetStatusCommand(budget.Id, new ChangeBudgetStatusDto(Guid.NewGuid())),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Status.NotFound);
    }

    [Fact]
    public async Task Handle_ValidStatusChange_UpdatesStatus()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var budget = TestDataSeeder.SeedBudget(db, companyId, customer.Id);

        var sut = new ChangeBudgetStatusCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new ChangeBudgetStatusCommand(budget.Id, new ChangeBudgetStatusDto(StatusIds.Budget.Approved)),
            CancellationToken.None);

        result.StatusId.Should().Be(StatusIds.Budget.Approved);
        db.Budgets.Single(b => b.Id == budget.Id).StatusId.Should().Be(StatusIds.Budget.Approved);
    }
}
