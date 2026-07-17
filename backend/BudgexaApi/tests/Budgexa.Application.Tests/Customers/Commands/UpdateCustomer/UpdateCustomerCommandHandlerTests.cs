namespace Budgexa.Application.Tests.Customers.Commands.UpdateCustomer;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.Commands.UpdateCustomer;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class UpdateCustomerCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    private static CustomerUpdateDto BuildDto(string taxId = "B12345678", string legalName = "Renamed") =>
        new(legalName, "Renamed", taxId, null, null, null, null, null, null, null, null);

    [Fact]
    public async Task Handle_UnknownCustomer_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new UpdateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateCustomerCommand(Guid.NewGuid(), BuildDto()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Customer.NotFound);
    }

    [Fact]
    public async Task Handle_CustomerFromAnotherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var otherCustomer = TestDataSeeder.SeedCustomer(db, Guid.NewGuid(), newStatusId, taxId: "X99999999");

        var sut = new UpdateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateCustomerCommand(otherCustomer.Id, BuildDto()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_DeletedCustomer_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        var deleted = TestDataSeeder.SeedCustomer(db, companyId, deleteStatusId, taxId: "X11111111");

        var sut = new UpdateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateCustomerCommand(deleted.Id, BuildDto()), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_DuplicateTaxId_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, taxId: "B00000000", legalName: "Other");
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, taxId: "B12345678", legalName: "Acme");

        var sut = new UpdateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new UpdateCustomerCommand(customer.Id, BuildDto(taxId: "B00000000")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Customer.TaxIdAlreadyExists);
    }

    [Fact]
    public async Task Handle_KeepingSameTaxId_DoesNotConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, taxId: "B12345678");

        var sut = new UpdateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new UpdateCustomerCommand(customer.Id, BuildDto(taxId: "B12345678", legalName: "Renamed")), CancellationToken.None);

        result.LegalName.Should().Be("Renamed");
        result.TaxId.Should().Be("B12345678");
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, taxId: "B12345678", legalName: "Acme");

        var sut = new UpdateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(
            new UpdateCustomerCommand(customer.Id, BuildDto(taxId: "B87654321", legalName: "Renamed")),
            CancellationToken.None);

        result.Id.Should().Be(customer.Id);
        result.LegalName.Should().Be("Renamed");
        result.TaxId.Should().Be("B87654321");
        db.Customers.Single(c => c.Id == customer.Id).TaxId.Should().Be("B87654321");
    }
}
