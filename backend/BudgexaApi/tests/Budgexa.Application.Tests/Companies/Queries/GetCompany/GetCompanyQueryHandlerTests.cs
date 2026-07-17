namespace Budgexa.Application.Tests.Companies.Queries.GetCompany;

using System.Net;
using Budgexa.Application.Companies.Queries.GetCompany;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Infrastructure.Persistence;

public class GetCompanyQueryHandlerTests
{
    private static Company SeedCompany(ApplicationDbContext db)
    {
        var company = Company.Create(
            "Test Company",
            "A test company description",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            Guid.NewGuid(),
            phone: "123-456-789",
            email: "test@example.com");
        db.Companies.Add(company);
        db.SaveChanges();
        return company;
    }

    [Fact]
    public async Task Handle_CompanyExists_ReturnsCompanyDto()
    {
        using var db = TestDbContextFactory.Create();
        var company = SeedCompany(db);

        var sut = new GetCompanyQueryHandler(db);

        var result = await sut.Handle(new GetCompanyQuery(company.Id), CancellationToken.None);

        result.Id.Should().Be(company.Id);
        result.Name.Should().Be("Test Company");
        result.Description.Should().Be("A test company description");
        result.Phone.Should().Be("123-456-789");
        result.Email.Should().Be("test@example.com");
        result.EndDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CompanyNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();

        var sut = new GetCompanyQueryHandler(db);

        var act = () => sut.Handle(new GetCompanyQuery(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Company.NotFound);
    }
}