namespace Budgexa.Application.Tests.Users.Queries.GetUserById;

using System.Net;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Queries.GetUserById;
using Budgexa.Domain.Exceptions;

public class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new GetUserByIdQueryHandler(db);

        var act = () => sut.Handle(new GetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.User.NotFound);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsDto()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "alice@example.com");

        var sut = new GetUserByIdQueryHandler(db);

        var result = await sut.Handle(new GetUserByIdQuery(user.Id), CancellationToken.None);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("alice@example.com");
        result.Company.Id.Should().Be(companyId);
        result.Language.Id.Should().Be(languageId);
    }
}
