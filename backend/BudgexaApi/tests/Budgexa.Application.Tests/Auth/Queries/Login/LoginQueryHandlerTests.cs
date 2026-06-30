namespace Budgexa.Application.Tests.Auth.Queries.Login;

using System.Net;
using Budgexa.Application.Auth;
using Budgexa.Application.Auth.Queries.Login;
using Budgexa.Application.Tests.TestHelpers;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Exceptions;
using Budgexa.Domain.Interfaces;
using NSubstitute;

public class LoginQueryHandlerTests
{
    private static (
        IJwtSettingsProvider jwt,
        ILoginLockoutSettingsProvider lockout) BuildSettings()
    {
        var jwt = Substitute.For<IJwtSettingsProvider>();
        jwt.RefreshTokenExpirationInDays.Returns(7);

        var lockout = Substitute.For<ILoginLockoutSettingsProvider>();
        lockout.MaxFailedAttempts.Returns(3);
        lockout.LockoutMinutes.Returns(15);

        return (jwt, lockout);
    }

    [Fact]
    public async Task Handle_UnknownEmail_ThrowsUnauthorized()
    {
        using var db = TestDbContextFactory.Create();
        TestDataSeeder.SeedReferenceData(db);

        var (jwt, lockout) = BuildSettings();
        var sut = new LoginQueryHandler(
            db,
            Substitute.For<IJwtTokenGenerator>(),
            Substitute.For<IPasswordHasher>(),
            jwt,
            lockout);

        var act = () => sut.Handle(new LoginQuery("missing@example.com", "x"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ex.Which.Tag.Should().Be(ErrorTags.Auth.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_LockedOutUser_ThrowsForbidden()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId);
        user.RegisterFailedLogin(maxAttempts: 1, lockoutDuration: TimeSpan.FromMinutes(10));
        await db.SaveChangesAsync();

        var (jwt, lockout) = BuildSettings();
        var sut = new LoginQueryHandler(
            db,
            Substitute.For<IJwtTokenGenerator>(),
            Substitute.For<IPasswordHasher>(),
            jwt,
            lockout);

        var act = () => sut.Handle(new LoginQuery(user.Email, "any"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        ex.Which.Tag.Should().Be(ErrorTags.Auth.AccountLocked);
    }

    [Fact]
    public async Task Handle_InvalidPassword_RegistersFailedLoginAndThrows()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, passwordHash: "stored");

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Verify("wrong", "stored").Returns(false);

        var (jwt, lockout) = BuildSettings();
        var sut = new LoginQueryHandler(
            db,
            Substitute.For<IJwtTokenGenerator>(),
            hasher,
            jwt,
            lockout);

        var act = () => sut.Handle(new LoginQuery(user.Email, "wrong"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        ex.Which.Tag.Should().Be(ErrorTags.Auth.InvalidCredentials);

        var refreshed = db.Users.Single(u => u.Id == user.Id);
        refreshed.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ExpiredContract_ThrowsForbidden()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);

        // Force the company contract to be expired by updating its end date.
        var company = db.Companies.Single(c => c.Id == companyId);
        company.Update(
            company.Name,
            company.Description,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            Guid.NewGuid());
        await db.SaveChangesAsync();

        var user = TestDataSeeder.SeedUser(db, companyId, languageId, newStatusId, passwordHash: "stored");

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Verify("right", "stored").Returns(true);

        var (jwt, lockout) = BuildSettings();
        var sut = new LoginQueryHandler(
            db,
            Substitute.For<IJwtTokenGenerator>(),
            hasher,
            jwt,
            lockout);

        var act = () => sut.Handle(new LoginQuery(user.Email, "right"), CancellationToken.None);

        var ex = await act.Should().ThrowAsync<AppException>();
        ex.Which.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        ex.Which.Tag.Should().Be(ErrorTags.Auth.ContractExpired);
    }

    [Fact]
    public async Task Handle_ValidLogin_ReturnsTokensAndPersistsRefreshToken()
    {
        using var db = TestDbContextFactory.Create();
        var (companyId, languageId, newStatusId, _) = TestDataSeeder.SeedReferenceData(db);
        var user = TestDataSeeder.SeedUser(
            db,
            companyId,
            languageId,
            newStatusId,
            email: "alice@example.com",
            firstName: "Alice",
            lastName: "Smith",
            passwordHash: "stored");

        var hasher = Substitute.For<IPasswordHasher>();
        hasher.Verify("right", "stored").Returns(true);

        var tokenGen = Substitute.For<IJwtTokenGenerator>();
        tokenGen.GenerateToken(Arg.Any<User>()).Returns("access-token");

        var (jwt, lockout) = BuildSettings();
        var sut = new LoginQueryHandler(db, tokenGen, hasher, jwt, lockout);

        var response = await sut.Handle(new LoginQuery("alice@example.com", "right"), CancellationToken.None);

        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be("alice@example.com");
        response.FullName.Should().Be("Alice Smith");
        response.AccessToken.Should().Be("access-token");
        response.RefreshToken.Should().NotBeNullOrWhiteSpace();

        db.RefreshTokens.Should().ContainSingle(rt => rt.UserId == user.Id && rt.Token == response.RefreshToken);
    }
}
