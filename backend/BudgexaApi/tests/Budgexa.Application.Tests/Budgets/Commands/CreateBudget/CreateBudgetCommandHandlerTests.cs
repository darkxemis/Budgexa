namespace Budgexa.Application.Tests.Budgets.Commands.CreateBudget;

using System.Net;
using Budgexa.Application.Budgets.Commands.CreateBudget;
using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class CreateBudgetCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static BudgetCreateDto BuildDto(
        Guid customerId,
        string number = "BUD-0001",
        List<BudgetLineUpsertDto>? lines = null)
    {
        return new BudgetCreateDto(
            customerId,
            number,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR",
            "Notes",
            "Terms",
            lines ?? new List<BudgetLineUpsertDto>
            {
                new(null, null, 1, "Service", "hour", 2m, 100m, 0m, 21m),
            });
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedBudgetStatuses(db);

        var sut = new CreateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new CreateBudgetCommand(BuildDto(Guid.NewGuid())), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Customer.NotFound);
    }

    [Fact]
    public async Task Handle_CustomerFromOtherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedBudgetStatuses(db);
        var foreignCustomer = TestDataSeeder.SeedCustomer(db, Guid.NewGuid(), newStatusId);

        var sut = new CreateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new CreateBudgetCommand(BuildDto(foreignCustomer.Id)), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_DuplicateBudgetNumberInSameCompany_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        TestDataSeeder.SeedBudget(db, companyId, customer.Id, number: "BUD-0001");

        var sut = new CreateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new CreateBudgetCommand(BuildDto(customer.Id, number: "BUD-0001")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Budget.NumberAlreadyExists);
    }

    [Fact]
    public async Task Handle_ValidBudget_CreatesWithLinesAndTotals()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedBudgetStatuses(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);

        var sut = new CreateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateBudgetCommand(BuildDto(customer.Id)), CancellationToken.None);

        result.Number.Should().Be("BUD-0001");
        result.CompanyId.Should().Be(companyId);
        result.CustomerId.Should().Be(customer.Id);
        result.StatusId.Should().Be(StatusIds.Budget.Draft);
        result.Lines.Should().ContainSingle();
        result.Subtotal.Should().Be(200m);
        result.TaxAmount.Should().Be(42m);
        result.Total.Should().Be(242m);
        db.Budgets.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_BudgetWithoutLines_CreatesWithZeroTotals()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedBudgetStatuses(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);

        var sut = new CreateBudgetCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new CreateBudgetCommand(BuildDto(customer.Id, lines: new List<BudgetLineUpsertDto>())),
            CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.Total.Should().Be(0);
    }
}
