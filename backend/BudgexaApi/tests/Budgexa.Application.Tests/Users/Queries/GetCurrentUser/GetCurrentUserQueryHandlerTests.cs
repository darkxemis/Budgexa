namespace Budgexa.Application.Tests.Users.Queries.GetCurrentUser;

using System.Net;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Application.Users.Queries.GetCurrentUser;
using Budgexa.Domain.Exceptions;
using NSubstitute;

public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task Handle_UnknownUser_ThrowsNotFound()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(Guid.NewGuid());

        var sut = new GetCurrentUserQueryHandler(db, current);

        var act = () => sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ex.Which.Tag.Should().Be(ErrorTags.User.NotFound);
    }

    [Fact]
    public async Task Handle_KnownUser_ReturnsProfile()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, email: "alice@example.com");

        var current = Substitute.For<ICurrentUserService>();
        current.UserId.Returns(user.Id);

        var sut = new GetCurrentUserQueryHandler(db, current);

        var profile = await sut.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        profile.Id.Should().Be(user.Id);
        profile.Email.Should().Be("alice@example.com");
        profile.CompanyId.Should().Be(companyId);
        profile.LanguageId.Should().Be(languageId);
    }
}
