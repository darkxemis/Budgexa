namespace Budgexa.Application.Tests.TestHelpers;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Infrastructure.Persistence;

/// <summary>
/// Seeds <see cref="ApplicationDbContext"/> instances with minimal reference data
/// (statuses, languages, default company) used by most handler tests.
/// </summary>
internal static class TestDataSeeder
{
    public const string DefaultCompanyName = "Test Company";

    public static (Guid companyId, Guid languageId, Guid newStatusId, Guid deleteStatusId) SeedReferenceData(ApplicationDbContext db)
    {
        var english = Language.Create("en", "English", LanguageIds.English);
        var spanish = Language.Create("es", "Spanish", LanguageIds.Spanish);
        db.Languages.Add(english);
        db.Languages.Add(spanish);

        var newStatus = Status.Create("User", "New", (int)Budgexa.Domain.Enums.BaseStatus.New, StatusIds.New);
        var deleteStatus = Status.Create("User", "Deleted", (int)Budgexa.Domain.Enums.BaseStatus.Delete, StatusIds.Delete);
        db.Statuses.Add(newStatus);
        db.Statuses.Add(deleteStatus);

        var company = Company.Create(
            DefaultCompanyName,
            "Default test company",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            null,
            Guid.NewGuid());
        db.Companies.Add(company);

        db.Roles.Add(Role.Create(RoleNames.Freelance, RoleIds.Freelance));
        db.Roles.Add(Role.Create(RoleNames.Administrator, RoleIds.Administrator));
        db.Roles.Add(Role.Create(RoleNames.SuperAdministrator, RoleIds.SuperAdministrator));

        db.SaveChanges();

        return (company.Id, english.Id, newStatus.Id, deleteStatus.Id);
    }

    public static User SeedUser(
        ApplicationDbContext db,
        Guid companyId,
        Guid languageId,
        Guid statusId,
        string email = "test@example.com",
        string passwordHash = "hashed-password",
        string firstName = "Test",
        string lastName = "User",
        IEnumerable<Guid>? roleIds = null)
    {
        var user = User.Create(email, passwordHash, firstName, lastName, companyId, languageId, statusId);

        if (roleIds is not null)
        {
            user.SetRoles(roleIds.ToList());
        }

        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }
}
