namespace Budgexa.Application.Tests.Invoices.Commands.DeleteInvoice;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Commands.DeleteInvoice;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class DeleteInvoiceCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        return current;
    }

    [Fact]
    public async Task Handle_UnknownInvoice_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new DeleteInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteInvoiceCommand(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.NotFound);
    }

    [Fact]
    public async Task Handle_InvoiceFromOtherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var otherCompanyId = Guid.NewGuid();
        var customer = TestDataSeeder.SeedCustomer(db, otherCompanyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, otherCompanyId, customer.Id);

        var sut = new DeleteInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteInvoiceCommand(invoice.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingInvoice_MarksAsDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id);

        var sut = new DeleteInvoiceCommandHandler(db, BuildCurrentUser(companyId));

        await sut.Handle(new DeleteInvoiceCommand(invoice.Id), CancellationToken.None);

        db.Invoices.Single(i => i.Id == invoice.Id).StatusId.Should().Be(StatusIds.Delete);
    }
}
