namespace Budgexa.Application.Tests.Budgets.Commands.UpdateBudget;

using System.Net;
using Budgexa.Application.Budgets.Commands.UpdateBudget;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class UpdateBudgetCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static BudgetUpdateDto BuildDto(
        Guid customerId,
        string number,
        List<BudgetLineUpsertDto>? lines = null)
    {
        return new BudgetUpdateDto(
            customerId,
            number,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR",
            "Updated",
            "Updated terms",
            lines ?? new List<BudgetLineUpsertDto>());
    }

    [Fact]
    public async Task Handle_UnknownBudget_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new UpdateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateBudgetCommand(Guid.NewGuid(), BuildDto(Guid.NewGuid(), "BUD-0001")), CancellationToken.None);

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

        var sut = new UpdateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateBudgetCommand(budget.Id, BuildDto(customer.Id, "BUD-1")), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var budget = TestDataSeeder.SeedBudget(db, companyId, customer.Id);

        var sut = new UpdateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateBudgetCommand(budget.Id, BuildDto(Guid.NewGuid(), "BUD-0001")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Customer.NotFound);
    }

    [Fact]
    public async Task Handle_DuplicateNumber_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: "BUD-001");
        var b2 = TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: "BUD-002");

        var sut = new UpdateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateBudgetCommand(b2.Id, BuildDto(customer.Id, "BUD-001")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Budget.NumberAlreadyExists);
    }

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesHeaderAndLines()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var budget = TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: "BUD-001");
        var existingLineId = budget.Lines.Single().Id;
        db.ChangeTracker.Clear();

        var dto = BuildDto(
            customer.Id,
            "BUD-001",
            new List<BudgetLineUpsertDto>
            {
                new(existingLineId, null, 1, "Updated line", "unit", 3m, 50m, 0m, 21m),
                new(null, null, 2, "New line", "unit", 1m, 25m, 0m, 21m),
            });

        var sut = new UpdateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new UpdateBudgetCommand(budget.Id, dto), CancellationToken.None);

        result.Lines.Should().HaveCount(2);
        result.Subtotal.Should().Be(175m);
        result.TaxAmount.Should().Be(36.75m);
        result.Total.Should().Be(211.75m);
        result.Notes.Should().Be("Updated");
    }

    [Fact]
    public async Task Handle_RemovesMissingLines()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var budget = TestDataSeeder.SeedBudget(db, companyId, customer.Id);
        db.ChangeTracker.Clear();

        var sut = new UpdateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new UpdateBudgetCommand(budget.Id, BuildDto(customer.Id, budget.Number, lines: new List<BudgetLineUpsertDto>())),
            CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.Total.Should().Be(0);
    }
}
