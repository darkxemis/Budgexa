namespace Budgexa.Application.Tests.Customers.Commands.CreateCustomer;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.Commands.CreateCustomer;
using Budgexa.Application.Customers.DTOs;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class CreateCustomerCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId, Guid? languageId = null)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(languageId ?? LanguageIds.English);
        return current;
    }

    private static CustomerCreateDto BuildDto(string taxId = "B12345678", string legalName = "Acme S.L.") =>
        new(legalName, "Acme", taxId, "billing@acme.test", "+34 600", "Address", "City", "ZIP", "Province", "ES", "Notes");

    [Fact]
    public async Task Handle_NewCustomer_CreatesAndReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new CreateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateCustomerCommand(BuildDto()), CancellationToken.None);

        result.LegalName.Should().Be("Acme S.L.");
        result.TaxId.Should().Be("B12345678");
        result.CompanyId.Should().Be(companyId);
        result.StatusId.Should().Be(StatusIds.New);

        db.Customers.Should().ContainSingle(c => c.TaxId == "B12345678" && c.CompanyId == companyId);
    }

    [Fact]
    public async Task Handle_DuplicateTaxIdInSameCompany_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, taxId: "B12345678");

        var sut = new CreateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new CreateCustomerCommand(BuildDto(taxId: "B12345678")), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.Customer.TaxIdAlreadyExists);
    }

    [Fact]
    public async Task Handle_SameTaxIdInDifferentCompany_IsAllowed()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var otherCompanyId = Guid.NewGuid();
        TestDataSeeder.SeedCustomer(db, otherCompanyId, newStatusId, taxId: "B12345678");

        var sut = new CreateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateCustomerCommand(BuildDto(taxId: "B12345678")), CancellationToken.None);

        result.CompanyId.Should().Be(companyId);
        db.Customers.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_DuplicateTaxIdOnDeletedCustomer_IsAllowed()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedCustomer(db, companyId, deleteStatusId, taxId: "B12345678");

        var sut = new CreateCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new CreateCustomerCommand(BuildDto(taxId: "B12345678")), CancellationToken.None);

        result.TaxId.Should().Be("B12345678");
    }
}
