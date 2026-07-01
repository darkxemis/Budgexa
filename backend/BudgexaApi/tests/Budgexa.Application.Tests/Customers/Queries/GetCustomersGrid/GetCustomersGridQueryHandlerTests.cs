namespace Budgexa.Application.Tests.Customers.Queries.GetCustomersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Customers.Queries.GetCustomersGrid;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using NSubstitute;

public class GetCustomersGridQueryHandlerTests
{
    private static ICurrentUserService BuildCurrentUser(Guid companyId)
    {
        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);
        current.GetLanguageIdAsync(Arg.Any<CancellationToken>()).Returns(LanguageIds.English);
        return current;
    }

    [Fact]
    public async Task Handle_ScopesByCompanyAndExcludesDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, deleteStatusId) = TestDataSeeder.SeedReferenceData(db);

        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha", taxId: "A1");
        TestDataSeeder.SeedCustomer(db, companyId, deleteStatusId, legalName: "Deleted", taxId: "A2");
        TestDataSeeder.SeedCustomer(db, Guid.NewGuid(), newStatusId, legalName: "Other", taxId: "A3");

        var sut = new GetCustomersGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetCustomersGridQuery(new GridRequestDto(1, 10, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Data.Should().ContainSingle(c => c.LegalName == "Alpha");
    }

    [Fact]
    public async Task Handle_AppliesSearchAcrossKeyColumns()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);

        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Alpha", taxId: "A1", email: "alpha@example.com", city: "Madrid", country: "ES");
        TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: "Beta", taxId: "B2", email: "beta@example.com", city: "Barcelona", country: "ES");

        var sut = new GetCustomersGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetCustomersGridQuery(new GridRequestDto(1, 10, null, null, "barcelona")),
            CancellationToken.None);

        response.TotalCount.Should().Be(1);
        response.Data.Should().ContainSingle(c => c.LegalName == "Beta");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSlice()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, _, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);

        for (var i = 0; i < 5; i++)
        {
            TestDataSeeder.SeedCustomer(db, companyId, newStatusId, legalName: $"Customer {i:D2}", taxId: $"T{i:D8}");
        }

        var sut = new GetCustomersGridQueryHandler(db, BuildCurrentUser(companyId));

        var response = await sut.Handle(
            new GetCustomersGridQuery(new GridRequestDto(2, 2, null, null, null)),
            CancellationToken.None);

        response.TotalCount.Should().Be(5);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(2);
        response.TotalPages.Should().Be(3);
        response.Data.Should().HaveCount(2);
    }
}
