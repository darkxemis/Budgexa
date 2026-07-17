namespace Budgexa.Application.Tests.Invoices.Commands.RegisterInvoicePayment;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Commands.RegisterInvoicePayment;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Enums;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class RegisterInvoicePaymentCommandHandlerTests
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
    public async Task Handle_UnknownInvoice_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new RegisterInvoicePaymentCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new RegisterInvoicePaymentCommand(Guid.NewGuid(), new RegisterInvoicePaymentDto(10m, PaymentMethod.Cash, null)),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.NotFound);
    }

    [Fact]
    public async Task Handle_PartialPayment_SetsPartiallyPaidStatus()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id);

        var sut = new RegisterInvoicePaymentCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new RegisterInvoicePaymentCommand(invoice.Id, new RegisterInvoicePaymentDto(50m, PaymentMethod.BankTransfer, "REF")),
            CancellationToken.None);

        result.AmountPaid.Should().Be(50m);
        result.AmountDue.Should().Be(invoice.Total - 50m);
        result.IsFullyPaid.Should().BeFalse();
        result.StatusId.Should().Be(StatusIds.Invoice.PartiallyPaid);
    }

    [Fact]
    public async Task Handle_FullPayment_SetsPaidStatus()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id);

        var sut = new RegisterInvoicePaymentCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new RegisterInvoicePaymentCommand(invoice.Id, new RegisterInvoicePaymentDto(invoice.Total, PaymentMethod.Cash, null)),
            CancellationToken.None);

        result.IsFullyPaid.Should().BeTrue();
        result.AmountDue.Should().Be(0m);
        result.StatusId.Should().Be(StatusIds.Invoice.Paid);
    }

    [Fact]
    public async Task Handle_AlreadyFullyPaid_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id);
        invoice.RegisterPayment(invoice.Total, PaymentMethod.Cash, null, Guid.NewGuid());
        await db.SaveChangesAsync(CancellationToken.None);

        var sut = new RegisterInvoicePaymentCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new RegisterInvoicePaymentCommand(invoice.Id, new RegisterInvoicePaymentDto(1m, PaymentMethod.Cash, null)),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.AlreadyPaid);
    }
}
