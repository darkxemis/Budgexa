namespace Budgexa.Application.Tests.Invoices.Commands.CreateInvoice;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Commands.CreateInvoice;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class CreateInvoiceCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static InvoiceCreateDto BuildDto(
        Guid customerId,
        string series = "A",
        string number = "INV-0001",
        Guid? budgetId = null,
        List<InvoiceLineUpsertDto>? lines = null)
    {
        return new InvoiceCreateDto(
            customerId,
            budgetId,
            series,
            number,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR",
            "Notes",
            lines ?? new List<InvoiceLineUpsertDto>
            {
                new(null, null, 1, "Service", "hour", 2m, 100m, 0m, 21m, 0m),
            });
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedInvoiceStatuses(db);

        var sut = new CreateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new CreateInvoiceCommand(BuildDto(Guid.NewGuid())), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Customer.NotFound);
    }

    [Fact]
    public async Task Handle_BudgetNotFoundForCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedInvoiceStatuses(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);

        var sut = new CreateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new CreateInvoiceCommand(BuildDto(customer.Id, budgetId: Guid.NewGuid())),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Budget.NotFound);
    }

    [Fact]
    public async Task Handle_DuplicateSeriesAndNumberInCompany_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-0001");

        var sut = new CreateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new CreateInvoiceCommand(BuildDto(customer.Id, series: "A", number: "INV-0001")),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.NumberAlreadyExists);
    }

    [Fact]
    public async Task Handle_SameNumberDifferentSeries_IsAllowed()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedInvoiceStatuses(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-0001");

        var sut = new CreateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new CreateInvoiceCommand(BuildDto(customer.Id, series: "B", number: "INV-0001")),
            CancellationToken.None);

        result.Series.Should().Be("B");
        result.Number.Should().Be("INV-0001");
    }

    [Fact]
    public async Task Handle_ValidInvoice_CreatesWithLinesAndTotals()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedInvoiceStatuses(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);

        var sut = new CreateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateInvoiceCommand(BuildDto(customer.Id)), CancellationToken.None);

        result.CompanyId.Should().Be(companyId);
        result.CustomerId.Should().Be(customer.Id);
        result.StatusId.Should().Be(StatusIds.Invoice.Draft);
        result.Lines.Should().ContainSingle();
        result.Subtotal.Should().Be(200m);
        result.TaxAmount.Should().Be(42m);
        result.WithholdingAmount.Should().Be(0m);
        result.Total.Should().Be(242m);
        result.AmountPaid.Should().Be(0m);
        result.AmountDue.Should().Be(242m);
        result.IsFullyPaid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_LinesWithWithholding_ApplyToTotal()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedInvoiceStatuses(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);

        var lines = new List<InvoiceLineUpsertDto>
        {
            new(null, null, 1, "Consulting", "hour", 1m, 1000m, 0m, 21m, 15m),
        };

        var sut = new CreateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateInvoiceCommand(BuildDto(customer.Id, lines: lines)), CancellationToken.None);

        result.Subtotal.Should().Be(1000m);
        result.TaxAmount.Should().Be(210m);
        result.WithholdingAmount.Should().Be(150m);
        result.Total.Should().Be(1060m);
    }
}
