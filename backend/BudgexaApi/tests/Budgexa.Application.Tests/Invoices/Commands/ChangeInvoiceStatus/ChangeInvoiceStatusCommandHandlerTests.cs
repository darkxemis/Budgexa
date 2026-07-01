namespace Budgexa.Application.Tests.Invoices.Commands.ChangeInvoiceStatus;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Commands.ChangeInvoiceStatus;
using Budgexa.Application.Invoices.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class ChangeInvoiceStatusCommandHandlerTests
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

        var sut = new ChangeInvoiceStatusCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new ChangeInvoiceStatusCommand(Guid.NewGuid(), new ChangeInvoiceStatusDto(StatusIds.Invoice.Issued)),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.NotFound);
    }

    [Fact]
    public async Task Handle_UnknownTargetStatus_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id);

        var sut = new ChangeInvoiceStatusCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(
            new ChangeInvoiceStatusCommand(invoice.Id, new ChangeInvoiceStatusDto(Guid.NewGuid())),
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
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id);

        var sut = new ChangeInvoiceStatusCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new ChangeInvoiceStatusCommand(invoice.Id, new ChangeInvoiceStatusDto(StatusIds.Invoice.Issued)),
            CancellationToken.None);

        result.StatusId.Should().Be(StatusIds.Invoice.Issued);
        db.Invoices.Single(i => i.Id == invoice.Id).StatusId.Should().Be(StatusIds.Invoice.Issued);
    }
}
