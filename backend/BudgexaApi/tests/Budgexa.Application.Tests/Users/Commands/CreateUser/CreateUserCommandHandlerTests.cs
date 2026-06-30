namespace Budgexa.Application.Tests.Users.Commands.CreateUser;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Commands.CreateUser;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using NSubstitute;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_NewUser_CreatesAndReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, _, _) = TestDataSeeder.SeedReferenceData(db);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Hash("Secret-123!").Returns("hashed");

        var current = Substitute.For<ICurrentUserService>();
        current.CompanyId.Returns(companyId);

        var sut = new CreateUserCommandHandler(db, hasher, current);
        var dto = new UserCreateDto(
            "alice@example.com",
            "Secret-123!",
            "Alice",
            "Smith",
            languageId,
            new List<Guid> { RoleIds.Freelance });

        var result = await sut.Handle(new CreateUserCommand(dto), CancellationToken.None);

        result.Email.Should().Be("alice@example.com");
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Smith");
        result.Company.Id.Should().Be(companyId);
        result.Language.Id.Should().Be(languageId);
        result.Roles.Should().ContainSingle(r => r.Id == RoleIds.Freelance);

        db.Users.Should().ContainSingle(u => u.Email == "alice@example.com" && u.PasswordHash == "hashed");
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "alice@example.com");

        var sut = new CreateUserCommandHandler(
            db,
            Substitute.For<IPasswordHasher>(),
            Substitute.For<ICurrentUserService>());

        var act = () => sut.Handle(new CreateUserCommand(
            new UserCreateDto("alice@example.com", "Secret-123!", "Alice", "Smith", languageId, new List<Guid>())),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.User.EmailAlreadyExists);
    }
}
