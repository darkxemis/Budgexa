namespace Budgexa.Application.Tests.Companies.Commands.DeleteCompany;

using System.Net;
using Budgexa.Application.Companies.Commands.DeleteCompany;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Infrastructure.Persistence;
using NSubstitute;

public class DeleteCompanyCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser()
    {
        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(Guid.NewGuid());
        return current;
    }

    private static Company SeedCompany(ApplicationDbContext db)
    {
        var company = Company.Create(
            "Test Company To Delete",
            "A test company",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            Guid.NewGuid());
        db.Companies.Add(company);
        db.SaveChanges();
        return company;
    }

    [Fact]
    public async Task Handle_CompanyExists_DeletesCompany()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);
        var company = SeedCompany(db);

        var sut = new DeleteCompanyCommandHandler(db, BuildCurrentUser());

        await sut.Handle(new DeleteCompanyCommand(company.Id), CancellationToken.None);

        db.Companies.Should().NotContain(c => c.Id == company.Id);
    }

    [Fact]
    public async Task Handle_CompanyNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new DeleteCompanyCommandHandler(db, BuildCurrentUser());

        var act = () => sut.Handle(new DeleteCompanyCommand(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Company.NotFound);
    }
}