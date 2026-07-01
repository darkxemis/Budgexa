namespace Budgexa.Application.Tests.Invoices.Commands.UpdateInvoice;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Commands.UpdateInvoice;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class UpdateInvoiceCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static InvoiceUpdateDto BuildDto(
        Guid customerId,
        string series,
        string number,
        List<InvoiceLineUpsertDto>? lines = null)
    {
        return new InvoiceUpdateDto(
            customerId,
            series,
            number,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            "EUR",
            "Updated",
            lines ?? new List<InvoiceLineUpsertDto>());
    }

    [Fact]
    public async Task Handle_UnknownInvoice_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new UpdateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new UpdateInvoiceCommand(Guid.NewGuid(), BuildDto(Guid.NewGuid(), "A", "INV-0001")),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.NotFound);
    }

    [Fact]
    public async Task Handle_DuplicateSeriesAndNumber_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-001");
        var inv2 = TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-002");

        var sut = new UpdateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new UpdateInvoiceCommand(inv2.Id, BuildDto(customer.Id, "A", "INV-001")),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.NumberAlreadyExists);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReplacesLinesAndRecalculatesTotals()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-001");
        var existingLineId = invoice.Lines.Single().Id;
        db.ChangeTracker.Clear();

        var dto = BuildDto(
            customer.Id,
            "A",
            "INV-001",
            new List<InvoiceLineUpsertDto>
            {
                new(existingLineId, null, 1, "Updated", "unit", 3m, 50m, 0m, 21m, 0m),
                new(null, null, 2, "Extra", "unit", 1m, 25m, 0m, 21m, 0m),
            });

        var sut = new UpdateInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new UpdateInvoiceCommand(invoice.Id, dto), CancellationToken.None);

        result.Lines.Should().HaveCount(2);
        result.Subtotal.Should().Be(175m);
        result.TaxAmount.Should().Be(36.75m);
        result.Total.Should().Be(211.75m);
    }
}
