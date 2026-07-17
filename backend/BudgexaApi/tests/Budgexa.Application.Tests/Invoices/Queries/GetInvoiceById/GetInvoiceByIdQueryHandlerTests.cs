namespace Budgexa.Application.Tests.Invoices.Queries.GetInvoiceById;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Queries.GetInvoiceById;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class GetInvoiceByIdQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    [Fact]
    public async Task Handle_UnknownInvoice_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new GetInvoiceByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetInvoiceByIdQuery(Guid.NewGuid()), CancellationToken.None);

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
        var foreign = TestDataSeeder.SeedInvoice(db, otherCompanyId, customer.Id);

        var sut = new GetInvoiceByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetInvoiceByIdQuery(foreign.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingInvoice_ReturnsDtoWithLines()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Acme");
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-001");

        var sut = new GetInvoiceByIdQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetInvoiceByIdQuery(invoice.Id), CancellationToken.None);

        result.Id.Should().Be(invoice.Id);
        result.Series.Should().Be("A");
        result.Number.Should().Be("INV-001");
        result.CustomerName.Should().Be("Acme");
        result.Lines.Should().ContainSingle();
    }
}
