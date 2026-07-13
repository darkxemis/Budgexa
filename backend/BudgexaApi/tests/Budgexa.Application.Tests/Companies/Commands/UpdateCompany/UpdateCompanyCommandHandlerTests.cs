namespace Budgexa.Application.Tests.Companies.Commands.UpdateCompany;

using System.Net;
using Budgexa.Application.Companies.Commands.UpdateCompany;
using Budgexa.Application.Companies.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Infrastructure.Persistence;
using NSubstitute;

public class UpdateCompanyCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser()
    {
        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(Guid.NewGuid());
        return current;
    }

    private static CompanyUpdateDto BuildDto(
        string name = "Updated Company",
        string? description = "New description",
        string? phone = "987-654-321",
        string? email = "updated@example.com",
        DateOnly? endDate = null) =>
        new(name, description, phone, email, endDate);

    private static Company SeedCompany(ApplicationDbContext db)
    {
        var company = Company.Create(
            "Original Company",
            "Old description",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            Guid.NewGuid());
        db.Companies.Add(company);
        db.SaveChanges();
        return company;
    }

    [Fact]
    public async Task Handle_ValidData_UpdatesAndReturnsCompany()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);
        var company = SeedCompany(db);

        var sut = new UpdateCompanyCommandHandler(db, BuildCurrentUser());

        var result = await sut.Handle(new UpdateCompanyCommand(company.Id, BuildDto()), CancellationToken.None);

        result.Name.Should().Be("Updated Company");
        result.Description.Should().Be("New description");
        result.Phone.Should().Be("987-654-321");
        result.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task Handle_CompanyNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new UpdateCompanyCommandHandler(db, BuildCurrentUser());

        var act = () => sut.Handle(new UpdateCompanyCommand(Guid.NewGuid(), BuildDto()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.Company.NotFound);
    }

    [Fact]
    public async Task Handle_CompanyNameEmpty_ThrowsArgumentException()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);
        var company = SeedCompany(db);

        var sut = new UpdateCompanyCommandHandler(db, BuildCurrentUser());

        var act = () => sut.Handle(new UpdateCompanyCommand(company.Id, BuildDto(name: "")), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}