namespace Budgexa.Infrastructure.Tests.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Infrastructure.Persistence;
using Budgexa.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

public class CurrentUserServiceTests
{
    private static ApplicationDbContext CreateDb(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (IHttpContextAccessor accessor, DefaultHttpContext context) BuildAccessor(ClaimsPrincipal? principal = null)
    {
        var context = new DefaultHttpContext();
        if (principal is not null)
        {
            context.User = principal;
        }

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return (accessor, context);
    }

    [Fact]
    public void UserId_NoClaims_ReturnsEmpty()
    {
        var (accessor, _) = BuildAccessor();
        using var db = CreateDb();
        var sut = new CurrentUserService(accessor, db);

        sut.UserId.Should().Be(Guid.Empty);
        sut.CompanyId.Should().Be(Guid.Empty);
        sut.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void UserId_FromSubClaim_IsParsed()
    {
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, "alice@example.com"),
            new Claim("company_id", companyId.ToString()),
            new Claim("role", "freelance"),
            new Claim("role", "administrator"),
        }, authenticationType: "Test");

        var (accessor, _) = BuildAccessor(new ClaimsPrincipal(identity));
        using var db = CreateDb();
        var sut = new CurrentUserService(accessor, db);

        sut.UserId.Should().Be(userId);
        sut.CompanyId.Should().Be(companyId);
        sut.Email.Should().Be("alice@example.com");
        sut.IsAuthenticated.Should().BeTrue();
        sut.Roles.Should().BeEquivalentTo(new[] { "freelance", "administrator" });
    }

    [Fact]
    public async Task GetLanguageIdAsync_PrefersHeaderOverDatabase()
    {
        using var db = CreateDb();
        var english = Language.Create("en", "English", LanguageIds.English);
        var spanish = Language.Create("es", "Spanish", LanguageIds.Spanish);
        db.Languages.AddRange(english, spanish);
        await db.SaveChangesAsync();

        var (accessor, context) = BuildAccessor();
        context.Request.Headers["X-Language-Code"] = "es";
        var sut = new CurrentUserService(accessor, db);

        var result = await sut.GetLanguageIdAsync();

        result.Should().Be(LanguageIds.Spanish);
    }

    [Fact]
    public async Task GetLanguageIdAsync_HeaderUnknown_FallsBackToUserLanguage()
    {
        using var db = CreateDb();
        var (companyId, languageId, statusId) = await SeedMinimalAsync(db);
        var user = User.Create("a@b.c", "h", "A", "B", companyId, languageId, statusId);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        }, authenticationType: "Test");
        var (accessor, context) = BuildAccessor(new ClaimsPrincipal(identity));
        context.Request.Headers["X-Language-Code"] = "fr";

        var sut = new CurrentUserService(accessor, db);

        var result = await sut.GetLanguageIdAsync();

        result.Should().Be(languageId);
    }

    [Fact]
    public async Task GetLanguageIdAsync_NoHeaderUnauthenticated_ReturnsEmpty()
    {
        using var db = CreateDb();
        var (accessor, _) = BuildAccessor();
        var sut = new CurrentUserService(accessor, db);

        (await sut.GetLanguageIdAsync()).Should().Be(Guid.Empty);
    }

    private static async Task<(Guid companyId, Guid languageId, Guid statusId)> SeedMinimalAsync(ApplicationDbContext db)
    {
        var english = Language.Create("en", "English", LanguageIds.English);
        db.Languages.Add(english);

        var status = Status.Create("User", "New", 1, StatusIds.New);
        db.Statuses.Add(status);

        var company = Company.Create(
            "Test Company",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            null,
            Guid.NewGuid());
        db.Companies.Add(company);

        await db.SaveChangesAsync();
        return (company.Id, english.Id, status.Id);
    }
}
