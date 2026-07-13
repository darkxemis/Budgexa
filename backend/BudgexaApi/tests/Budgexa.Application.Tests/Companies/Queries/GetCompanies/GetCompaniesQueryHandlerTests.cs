namespace Budgexa.Application.Tests.Companies.Queries.GetCompanies;

using Budgexa.Application.Companies.Queries.GetCompanies;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Infrastructure.Persistence;

public class GetCompaniesQueryHandlerTests
{
    private static Company SeedCompany(ApplicationDbContext db, string name = "Test Company")
    {
        var company = Company.Create(
            name,
            "Description",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            Guid.NewGuid());
        db.Companies.Add(company);
        db.SaveChanges();
        return company;
    }

    [Fact]
    public async Task Handle_CompaniesExist_ReturnsCompanyDtos()
    {
        using var db = TestDbContextFactory.Create();
        SeedCompany(db, "Test Company 1");
        SeedCompany(db, "Test Company 2");

        var sut = new GetCompaniesQueryHandler(db);

        var result = await sut.Handle(new GetCompaniesQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Name == "Test Company 1");
        result.Should().Contain(c => c.Name == "Test Company 2");
    }

    [Fact]
    public async Task Handle_NoCompanies_ReturnsEmptyList()
    {
        using var db = TestDbContextFactory.Create();

        var sut = new GetCompaniesQueryHandler(db);

        var result = await sut.Handle(new GetCompaniesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}