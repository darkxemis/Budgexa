namespace Budgexa.Application.Tests.Customers.Commands.DeleteCustomer;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.Commands.DeleteCustomer;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class DeleteCustomerCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.UserId.Returns(Guid.NewGuid());
        return current;
    }

    [Fact]
    public async Task Handle_UnknownCustomer_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, _, _) = TestDataSeeder.SeedReferenceData(db);

        var sut = new DeleteCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteCustomerCommand(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Customer.NotFound);
    }

    [Fact]
    public async Task Handle_CustomerFromAnotherCompany_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var other = TestDataSeeder.SeedCustomer(db, Guid.NewGuid(), newStatusId);

        var sut = new DeleteCustomerCommandHandler(db, BuildCurrentUser(companyId));

        var act = () => sut.Handle(new DeleteCustomerCommand(other.Id), CancellationToken.None);

        await act.Should().ThrowAsync<AppException>();
    }

    [Fact]
    public async Task Handle_ExistingCustomer_MarksAsDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var customer = TestDataSeeder.SeedCustomer(db, companyId, newStatusId);

        var sut = new DeleteCustomerCommandHandler(db, BuildCurrentUser(companyId));

        await sut.Handle(new DeleteCustomerCommand(customer.Id), CancellationToken.None);

        db.Customers.Single(c => c.Id == customer.Id).StatusId.Should().Be(StatusIds.Delete);
    }
}
