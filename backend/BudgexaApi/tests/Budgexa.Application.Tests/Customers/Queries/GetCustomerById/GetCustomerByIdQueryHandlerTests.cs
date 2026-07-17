namespace Budgexa.Application.Tests.Customers.Queries.GetCustomerById;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.Queries.GetCustomerById;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class GetCustomerByIdQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    [Fact]
    public async Task Handle_UnknownCustomer_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new GetCustomerByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetCustomerByIdQuery(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Customer.NotFound);
    }

    [Fact]
    public async Task Handle_DeletedCustomer_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, deleteStatusId);

        var sut = new GetCustomerByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_CustomerInAnotherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var foreign = TestDataSeeder.SeedCustomer(db, Guid.NewGuid(), newStatusId);

        var sut = new GetCustomerByIdQueryHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new GetCustomerByIdQuery(foreign.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingCustomer_ReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Acme S.L.", taxId: "B12345678");

        var sut = new GetCustomerByIdQueryHandler(db, BuildCurrentUser(companyId));

        var result = await sut.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        result.Id.Should().Be(customer.Id);
        result.LegalName.Should().Be("Acme S.L.");
        result.TaxId.Should().Be("B12345678");
        result.CompanyId.Should().Be(companyId);
        result.StatusId.Should().Be(newStatusId);
    }
}
