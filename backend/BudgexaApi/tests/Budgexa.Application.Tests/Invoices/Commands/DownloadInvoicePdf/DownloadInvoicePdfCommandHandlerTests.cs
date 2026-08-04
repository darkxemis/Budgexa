namespace Budgexa.Application.Tests.Invoices.Commands.DownloadInvoicePdf;

using System.Net;
using System.Text;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Invoices.Commands.DownloadInvoicePdf;
using Budgexa.Application.Invoices.Services;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class DownloadInvoicePdfCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId, Guid userId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(userId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static IFileStorageService BuildFileStorage(byte[]? signatureBytes = null)
    {
        var fileStorage = Substitute.For<IFileStorageService>();
        fileStorage
            .GetSignatureImageBytesAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(signatureBytes ?? Encoding.UTF8.GetBytes("signature"));
        return fileStorage;
    }

    [Fact]
    public async Task Handle_UnknownInvoice_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new DownloadInvoicePdfCommandHandler(
            db,
            BuildCurrentUser(companyId, Guid.NewGuid()),
            Substitute.For<IInvoicePdfService>(),
            BuildFileStorage());

        var act = () => sut.Handle(new DownloadInvoicePdfCommand(Guid.NewGuid()), CancellationToken.None);

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

        var sut = new DownloadInvoicePdfCommandHandler(
            db,
            BuildCurrentUser(companyId, Guid.NewGuid()),
            Substitute.For<IInvoicePdfService>(),
            BuildFileStorage());

        var act = () => sut.Handle(new DownloadInvoicePdfCommand(invoice.Id), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.Invoice.NotFound);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id);

        var sut = new DownloadInvoicePdfCommandHandler(
            db,
            BuildCurrentUser(companyId, Guid.NewGuid()),
            Substitute.For<IInvoicePdfService>(),
            BuildFileStorage());

        var act = () => sut.Handle(new DownloadInvoicePdfCommand(invoice.Id), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.Tag.Should().Be(ErrorTags.User.NotFound);
    }

    [Fact]
    public async Task Handle_ExistingInvoice_GeneratesPdf()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);
        var invoice = TestDataSeeder.SeedInvoice(db, companyId, customer.Id, series: "A", number: "INV-0001");

        var signatureBytes = Encoding.UTF8.GetBytes("signature");
        var pdfService = Substitute.For<IInvoicePdfService>();
        pdfService.GeneratePdf(Arg.Any<Invoice>(), Arg.Any<Company>(), Arg.Any<Customer>(), Arg.Any<byte[]?>(), Arg.Any<string>())
            .Returns(Encoding.UTF8.GetBytes("pdf-content"));

        var sut = new DownloadInvoicePdfCommandHandler(
            db,
            BuildCurrentUser(companyId, user.Id),
            pdfService,
            BuildFileStorage(signatureBytes));

        var result = await sut.Handle(new DownloadInvoicePdfCommand(invoice.Id), CancellationToken.None);

        pdfService.Received(1).GeneratePdf(
            Arg.Is<Invoice>(i => i.Id == invoice.Id),
            Arg.Is<Company>(c => c.Id == companyId),
            Arg.Is<Customer>(c => c.Id == customer.Id),
            Arg.Is<byte[]>(b => b.SequenceEqual(signatureBytes)),
            "en");

        result.FileName.Should().Be($"Invoice_A_INV-0001_{invoice.IssueDate:yyyyMMdd}.pdf");
        result.PdfBytes.Should().NotBeNull();
    }
}
