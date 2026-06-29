namespace Budgexa.Application.Tests.Users.Commands.DeleteUser;

using System.Net;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Commands.DeleteUser;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;

public class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new DeleteUserCommandHandler(db);

        var act = () => sut.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.User.NotFound);
    }

    [Fact]
    public async Task Handle_ExistingUser_MarksAsDeleted()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId);

        var sut = new DeleteUserCommandHandler(db);

        await sut.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

        db.Users.Single(u => u.Id == user.Id).StatusId.Should().Be(StatusIds.Delete);
    }
}
