namespace Budgexa.Application.Tests.Auth.Commands.Register;

using System.Net;
using Budgexa.Application.Auth.Commands.Register;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

public class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_NewUser_CreatesUserAndPersistsRoles()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, _, _) = TestDataSeeder.SeedReferenceData(db);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Hash("Secret-123!").Returns("hashed");

        var sut = new RegisterCommandHandler(db, hasher);
        var command = new RegisterCommand(
            "alice@example.com",
            "Secret-123!",
            "Alice",
            "Smith",
            companyId,
            languageId,
            new[] { RoleIds.Freelance });

        var id = await sut.Handle(command, CancellationToken.None);

        var created = await db.Users.AsNoTracking()
            .Include(u => u.UserRoles)
            .FirstAsync(u => u.Id == id);
        created.Email.Should().Be("alice@example.com");
        created.PasswordHash.Should().Be("hashed");
        created.FirstName.Should().Be("Alice");
        created.LastName.Should().Be("Smith");
        created.CompanyId.Should().Be(companyId);
        created.LanguageId.Should().Be(languageId);
        created.StatusId.Should().Be(StatusIds.New);
        created.UserRoles.Should().ContainSingle(ur => ur.RoleId == RoleIds.Freelance);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "alice@example.com");

        var hasher = Substitute.For<IPasswordHasher>();
        var sut = new RegisterCommandHandler(db, hasher);

        var act = () => sut.Handle(
            new RegisterCommand(
                "alice@example.com",
                "Secret-123!",
                "Alice",
                "Smith",
                companyId,
                languageId,
                Array.Empty<Guid>()),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.User.EmailAlreadyExists);
    }
}
