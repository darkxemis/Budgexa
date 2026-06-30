namespace Budgexa.Application.Tests.Users.Commands.UpdateCurrentUser;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Commands.UpdateCurrentUser;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using NSubstitute;

public class UpdateCurrentUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(Guid.NewGuid());

        var sut = new UpdateCurrentUserCommandHandler(db, Substitute.For<IPasswordHasher>(), current);

        var act = () => sut.Handle(new UpdateCurrentUserCommand(
            new UpdateCurrentUserDto("F", "L", string.Empty, Guid.NewGuid())),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.User.NotFound);
    }

    [Fact]
    public async Task Handle_EmptyPassword_KeepsHashAndUpdatesProfile()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, passwordHash: "kept");

        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(user.Id);

        var hasher = Substitute.For<IPasswordHasher>();

        var sut = new UpdateCurrentUserCommandHandler(db, hasher, current);

        var result = await sut.Handle(new UpdateCurrentUserCommand(
            new UpdateCurrentUserDto("Alex", "Brown", string.Empty, languageId)),
            CancellationToken.None);

        hasher.DidNotReceiveWithAnyArgs().Hash(default!);
        result.FirstName.Should().Be("Alex");
        result.LastName.Should().Be("Brown");
        db.Users.Single(u => u.Id == user.Id).PasswordHash.Should().Be("kept");
    }

    [Fact]
    public async Task Handle_PasswordProvided_HashesAndPersists()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, passwordHash: "old");

        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(user.Id);

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Hash("Brand-NEW-1!").Returns("fresh-hash");

        var sut = new UpdateCurrentUserCommandHandler(db, hasher, current);

        await sut.Handle(new UpdateCurrentUserCommand(
            new UpdateCurrentUserDto("Alex", "Brown", "Brand-NEW-1!", languageId)),
            CancellationToken.None);

        db.Users.Single(u => u.Id == user.Id).PasswordHash.Should().Be("fresh-hash");
    }
}
