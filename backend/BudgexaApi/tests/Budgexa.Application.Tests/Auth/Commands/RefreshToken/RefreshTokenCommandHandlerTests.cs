namespace Budgexa.Application.Tests.Auth.Commands.RefreshToken;

using System.Net;
using Budgexa.Application.Auth;
using Budgexa.Application.Auth.Commands.RefreshToken;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using NSubstitute;
using DomainRefreshToken = Budgexa.Domain.Entities.RefreshToken;

public class RefreshTokenCommandHandlerTests
{
    private static IJwtSettingsProvider BuildJwtSettings(int days = 7)
    {
        var jwt = Substitute.For<IJwtSettingsProvider>();
        jwt.RefreshTokenExpirationInDays.Returns(days);
        return jwt;
    }

    [Fact]
    public async Task Handle_UnknownToken_ThrowsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var sut = new RefreshTokenCommandHandler(
            db,
            Substitute.For<IJwtTokenGenerator>(),
            BuildJwtSettings());

        var act = () => sut.Handle(new RefreshTokenCommand("missing"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ex.Which.Tag.Should().Be(ErrorTags.Auth.InvalidRefreshToken);
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId);

        var token = DomainRefreshToken.Create(user.Id, expirationInDays: 7);
        token.Revoke();
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        var sut = new RefreshTokenCommandHandler(
            db,
            Substitute.For<IJwtTokenGenerator>(),
            BuildJwtSettings());

        var act = () => sut.Handle(new RefreshTokenCommand(token.Token), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ex.Which.Tag.Should().Be(ErrorTags.Auth.InvalidRefreshToken);
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesOldTokenAndIssuesNewPair()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId);

        var token = DomainRefreshToken.Create(user.Id, expirationInDays: 7);
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        var tokenGen = Substitute.For<IJwtTokenGenerator>();
        tokenGen.GenerateToken(Arg.Any<User>()).Returns("new-access");

        var sut = new RefreshTokenCommandHandler(db, tokenGen, BuildJwtSettings());

        var response = await sut.Handle(new RefreshTokenCommand(token.Token), CancellationToken.None);

        response.AccessToken.Should().Be("new-access");
        response.UserId.Should().Be(user.Id);
        response.RefreshToken.Should().NotBe(token.Token);

        var stored = db.RefreshTokens.ToList();
        stored.Should().HaveCount(2);
        var oldToken = stored.Single(rt => rt.Token == token.Token);
        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedByToken.Should().Be(response.RefreshToken);
        stored.Should().ContainSingle(rt => rt.Token == response.RefreshToken && rt.IsActive);
    }

    [Fact]
    public async Task Handle_ValidTokenButMissingUser_ThrowsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var token = DomainRefreshToken.Create(Guid.NewGuid(), expirationInDays: 7);
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync();

        var sut = new RefreshTokenCommandHandler(
            db,
            Substitute.For<IJwtTokenGenerator>(),
            BuildJwtSettings());

        var act = () => sut.Handle(new RefreshTokenCommand(token.Token), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ex.Which.Tag.Should().Be(ErrorTags.Auth.InvalidRefreshToken);
    }
}
