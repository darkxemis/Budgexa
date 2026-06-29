namespace Budgexa.Application.Tests.Users.Commands.UpdateUser;

using System.Net;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Commands.UpdateUser;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using NSubstitute;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new UpdateUserCommandHandler(db, Substitute.For<IPasswordHasher>());

        var act = () => sut.Handle(new UpdateUserCommand(
            Guid.NewGuid(),
            new UserUpdateDto("a@a.com", string.Empty, "A", "B", Guid.NewGuid(), new List<Guid>())),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.User.NotFound);
    }

    [Fact]
    public async Task Handle_EmailChangedToTakenAddress_ThrowsConflict()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var alice = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "alice@example.com");
        TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "bob@example.com");

        var sut = new UpdateUserCommandHandler(db, Substitute.For<IPasswordHasher>());

        var act = () => sut.Handle(new UpdateUserCommand(
            alice.Id,
            new UserUpdateDto("bob@example.com", string.Empty, "Alice", "Smith", languageId, new List<Guid>())),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Conflict);
        ex.Which.Tag.Should().Be(ErrorTags.User.EmailAlreadyExists);
    }

    [Fact]
    public async Task Handle_EmptyPassword_KeepsExistingPasswordAndUpdatesProfile()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, passwordHash: "original");

        var hasher = Substitute.For<IPasswordHasher>();
        var sut = new UpdateUserCommandHandler(db, hasher);

        var result = await sut.Handle(new UpdateUserCommand(
            user.Id,
            new UserUpdateDto(user.Email, string.Empty, "NewFirst", "NewLast", languageId, new List<Guid> { RoleIds.Freelance })),
            CancellationToken.None);

        hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        result.FirstName.Should().Be("NewFirst");
        result.LastName.Should().Be("NewLast");
        result.Roles.Should().ContainSingle(r => r.Id == RoleIds.Freelance);
        db.Users.Single(u => u.Id == user.Id).PasswordHash.Should().Be("original");
    }

    [Fact]
    public async Task Handle_PasswordProvided_HashesAndStoresNewPassword()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, passwordHash: "original");

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Hash("NewSecret-123!").Returns("new-hash");

        var sut = new UpdateUserCommandHandler(db, hasher);

        var result = await sut.Handle(new UpdateUserCommand(
            user.Id,
            new UserUpdateDto(user.Email, "NewSecret-123!", "First", "Last", languageId, new List<Guid>())),
            CancellationToken.None);

        result.Should().NotBeNull();
        db.Users.Single(u => u.Id == user.Id).PasswordHash.Should().Be("new-hash");
    }
}
