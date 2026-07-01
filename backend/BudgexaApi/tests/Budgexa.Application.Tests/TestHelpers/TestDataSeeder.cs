namespace Budgexa.Application.Tests.TestHelpers;

using Budgexa.Domain.Constants;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
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

    public static void SeedBudgetStatuses(ApplicationDbContext db)
    {
        EnsureStatus(db, "Budget", "Draft", 1, StatusIds.Budget.Draft);
        EnsureStatus(db, "Budget", "Sent", 2, StatusIds.Budget.Sent);
        EnsureStatus(db, "Budget", "Approved", 3, StatusIds.Budget.Approved);
        db.SaveChanges();
    }

    public static void SeedInvoiceStatuses(ApplicationDbContext db)
    {
        EnsureStatus(db, "Invoice", "Draft", 1, StatusIds.Invoice.Draft);
        EnsureStatus(db, "Invoice", "Issued", 2, StatusIds.Invoice.Issued);
        EnsureStatus(db, "Invoice", "PartiallyPaid", 3, StatusIds.Invoice.PartiallyPaid);
        EnsureStatus(db, "Invoice", "Paid", 4, StatusIds.Invoice.Paid);
        db.SaveChanges();
    }

    private static void EnsureStatus(ApplicationDbContext db, string group, string name, int value, Guid id)
    {
        if (db.Statuses.Local.Any(s => s.Id == id) || db.Statuses.Any(s => s.Id == id))
        {
            return;
        }

        db.Statuses.Add(Status.Create(group, name, value, id));
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

    public static Customer SeedCustomer(
        ApplicationDbContext db,
        Guid companyId,
        Guid statusId,
        string legalName = "Test Customer S.L.",
        string? tradeName = null,
        string taxId = "B12345678",
        string? email = null,
        string? phone = null,
        string? city = null,
        string? country = null)
    {
        var customer = Customer.Create(
            companyId,
            statusId,
            legalName,
            tradeName,
            taxId,
            email,
            phone,
            addressLine: null,
            city: city,
            postalCode: null,
            province: null,
            country: country,
            notes: null,
            createdByUserId: Guid.NewGuid());

        db.Customers.Add(customer);
        db.SaveChanges();
        return customer;
    }

    public static Item SeedItem(
        ApplicationDbContext db,
        Guid companyId,
        Guid statusId,
        string? sku = "SKU-001",
        string name = "Test Item",
        ItemType type = ItemType.Service,
        string unit = "unit",
        decimal unitPrice = 100m,
        decimal taxRate = 21m,
        string currency = "EUR")
    {
        var item = Item.Create(
            companyId,
            statusId,
            sku,
            name,
            description: null,
            type,
            unit,
            unitPrice,
            taxRate,
            currency,
            createdByUserId: Guid.NewGuid());

        db.Items.Add(item);
        db.SaveChanges();
        return item;
    }

    public static Budget SeedBudget(
        ApplicationDbContext db,
        Guid companyId,
        Guid customerId,
        Guid? statusId = null,
        string number = "BUD-0001",
        string currency = "EUR",
        decimal lineQuantity = 1m,
        decimal lineUnitPrice = 100m,
        decimal lineTaxRate = 21m,
        bool withLine = true)
    {
        SeedBudgetStatuses(db);

        var budget = Budget.Create(
            companyId,
            customerId,
            statusId ?? StatusIds.Budget.Draft,
            number,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            currency,
            notes: null,
            termsAndConditions: null,
            createdByUserId: Guid.NewGuid());

        if (withLine)
        {
            budget.AddLine(
                itemId: null,
                sortOrder: 1,
                description: "Default line",
                unit: "unit",
                quantity: lineQuantity,
                unitPrice: lineUnitPrice,
                discountPercentage: 0m,
                taxRate: lineTaxRate,
                updatedByUserId: Guid.NewGuid());
        }

        db.Budgets.Add(budget);
        db.SaveChanges();
        return budget;
    }

    public static Invoice SeedInvoice(
        ApplicationDbContext db,
        Guid companyId,
        Guid customerId,
        Guid? statusId = null,
        string series = "A",
        string number = "INV-0001",
        string currency = "EUR",
        Guid? budgetId = null,
        decimal lineQuantity = 1m,
        decimal lineUnitPrice = 100m,
        decimal lineTaxRate = 21m,
        decimal lineWithholdingRate = 0m,
        bool withLine = true)
    {
        SeedInvoiceStatuses(db);

        var invoice = Invoice.Create(
            companyId,
            customerId,
            statusId ?? StatusIds.Invoice.Draft,
            series,
            number,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            currency,
            notes: null,
            createdByUserId: Guid.NewGuid(),
            budgetId: budgetId);

        if (withLine)
        {
            invoice.AddLine(
                itemId: null,
                sortOrder: 1,
                description: "Default line",
                unit: "unit",
                quantity: lineQuantity,
                unitPrice: lineUnitPrice,
                discountPercentage: 0m,
                taxRate: lineTaxRate,
                withholdingRate: lineWithholdingRate,
                updatedByUserId: Guid.NewGuid());
        }

        db.Invoices.Add(invoice);
        db.SaveChanges();
        return invoice;
    }
}
