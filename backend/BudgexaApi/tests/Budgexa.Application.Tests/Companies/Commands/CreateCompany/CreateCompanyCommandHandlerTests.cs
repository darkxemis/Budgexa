namespace Budgexa.Application.Tests.Companies.Commands.CreateCompany;

using Budgexa.Application.Companies.Commands.CreateCompany;
using Budgexa.Application.Companies.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using NSubstitute;

public class CreateCompanyCommandHandlerTests
{
    private static ICurrentUserService BuildCurrentUser()
    {
        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(Guid.NewGuid());
        return current;
    }

    private static CompanyCreateDto BuildDto(
        string name = "Test Company",
        string? description = "A test company for testing purposes",
        string? phone = "123-456-789",
        string? email = "test@example.com") =>
        new(name, description, phone, email, DateOnly.FromDateTime(DateTime.UtcNow), null);

    [Fact]
    public async Task Handle_ValidData_CreatesAndReturnsCompany()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new CreateCompanyCommandHandler(db, BuildCurrentUser());

        var result = await sut.Handle(new CreateCompanyCommand(BuildDto()), CancellationToken.None);

        result.Name.Should().Be("Test Company");
        result.Description.Should().Be("A test company for testing purposes");
        result.Phone.Should().Be("123-456-789");
        result.Email.Should().Be("test@example.com");
        result.EndDate.Should().BeNull();

        db.Companies.Should().Contain(c => c.Name == "Test Company");
    }

    [Fact]
    public async Task Handle_CompanyNameEmpty_ThrowsArgumentException()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new CreateCompanyCommandHandler(db, BuildCurrentUser());

        var act = () => sut.Handle(new CreateCompanyCommand(BuildDto(name: "")), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}