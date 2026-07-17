namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class BudgetTests
{
    private static Budget ValidBudget(Guid? id = null) =>
        Budget.Create(
            companyId: Guid.NewGuid(),
            customerId: Guid.NewGuid(),
            statusId: Guid.NewGuid(),
            number: "BUD-0001",
            issueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            validUntil: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            currency: "EUR",
            notes: "Notes",
            termsAndConditions: "Terms",
            createdByUserId: Guid.NewGuid(),
            id: id);

    [Fact]
    public void Create_WithValidArguments_InitializesProperties()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var validUntil = issueDate.AddDays(30);

        var budget = Budget.Create(
            companyId,
            customerId,
            statusId,
            "BUD-0001",
            issueDate,
            validUntil,
            "EUR",
            "Some notes",
            "Some terms",
            creatorId);

        budget.Id.Should().NotBe(Guid.Empty);
        budget.CompanyId.Should().Be(companyId);
        budget.CustomerId.Should().Be(customerId);
        budget.StatusId.Should().Be(statusId);
        budget.Number.Should().Be("BUD-0001");
        budget.IssueDate.Should().Be(issueDate);
        budget.ValidUntil.Should().Be(validUntil);
        budget.Currency.Should().Be("EUR");
        budget.Notes.Should().Be("Some notes");
        budget.TermsAndConditions.Should().Be("Some terms");
        budget.CreatedByUserId.Should().Be(creatorId);
        budget.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        budget.Lines.Should().BeEmpty();
        budget.Subtotal.Should().Be(0);
        budget.TaxAmount.Should().Be(0);
        budget.Total.Should().Be(0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithBlankNumber_ThrowsArgumentException(string? number)
    {
        var act = () => Budget.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            number!,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            "EUR",
            null,
            null,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Create_WithInvalidCurrency_ThrowsArgumentException(string currency)
    {
        var act = () => Budget.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BUD-0001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            currency,
            null,
            null,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithValidUntilBeforeIssueDate_ThrowsArgumentException()
    {
        var issueDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var act = () => Budget.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BUD-0001",
            issueDate,
            issueDate.AddDays(-1),
            "EUR",
            null,
            null,
            Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateHeader_WithValidArguments_ReplacesFieldsAndStampsUpdater()
    {
        var budget = ValidBudget();
        var newCustomerId = Guid.NewGuid();
        var updaterId = Guid.NewGuid();
        var newIssue = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        budget.UpdateHeader(
            newCustomerId,
            "BUD-0002",
            newIssue,
            newIssue.AddDays(15),
            "USD",
            "New notes",
            "New terms",
            updaterId);

        budget.CustomerId.Should().Be(newCustomerId);
        budget.Number.Should().Be("BUD-0002");
        budget.IssueDate.Should().Be(newIssue);
        budget.ValidUntil.Should().Be(newIssue.AddDays(15));
        budget.Currency.Should().Be("USD");
        budget.Notes.Should().Be("New notes");
        budget.TermsAndConditions.Should().Be("New terms");
        budget.UpdatedByUserId.Should().Be(updaterId);
        budget.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddLine_AddsLineAndRecalculatesTotals()
    {
        var budget = ValidBudget();
        var updaterId = Guid.NewGuid();

        var line = budget.AddLine(
            itemId: null,
            sortOrder: 1,
            description: "Service",
            unit: "hour",
            quantity: 2m,
            unitPrice: 100m,
            discountPercentage: 0m,
            taxRate: 21m,
            updatedByUserId: updaterId);

        budget.Lines.Should().ContainSingle();
        budget.Lines.Single().Id.Should().Be(line.Id);
        budget.Subtotal.Should().Be(200m);
        budget.TaxAmount.Should().Be(42m);
        budget.Total.Should().Be(242m);
        budget.UpdatedByUserId.Should().Be(updaterId);
        line.BudgetId.Should().Be(budget.Id);
    }

    [Fact]
    public void AddLine_WithDiscount_AppliesDiscountBeforeTax()
    {
        var budget = ValidBudget();

        budget.AddLine(
            null,
            1,
            "Service",
            "unit",
            quantity: 1m,
            unitPrice: 100m,
            discountPercentage: 10m,
            taxRate: 21m,
            updatedByUserId: Guid.NewGuid());

        budget.Subtotal.Should().Be(90m);
        budget.TaxAmount.Should().Be(18.90m);
        budget.Total.Should().Be(108.90m);
    }

    [Fact]
    public void UpdateLine_ChangesValuesAndRecalculatesTotals()
    {
        var budget = ValidBudget();
        var line = budget.AddLine(null, 1, "Service", "hour", 1m, 100m, 0m, 21m, Guid.NewGuid());

        budget.UpdateLine(
            line.Id,
            itemId: null,
            sortOrder: 1,
            description: "Updated",
            unit: "hour",
            quantity: 3m,
            unitPrice: 50m,
            discountPercentage: 0m,
            taxRate: 21m,
            updatedByUserId: Guid.NewGuid());

        budget.Lines.Single().Description.Should().Be("Updated");
        budget.Subtotal.Should().Be(150m);
        budget.TaxAmount.Should().Be(31.50m);
        budget.Total.Should().Be(181.50m);
    }

    [Fact]
    public void UpdateLine_WithUnknownId_ThrowsInvalidOperation()
    {
        var budget = ValidBudget();

        var act = () => budget.UpdateLine(
            Guid.NewGuid(),
            null, 1, "X", "u", 1m, 1m, 0m, 0m,
            Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveLine_RemovesAndRecalculates()
    {
        var budget = ValidBudget();
        var l1 = budget.AddLine(null, 1, "A", "u", 1m, 100m, 0m, 21m, Guid.NewGuid());
        var l2 = budget.AddLine(null, 2, "B", "u", 2m, 50m, 0m, 21m, Guid.NewGuid());

        budget.RemoveLine(l1.Id, Guid.NewGuid());

        budget.Lines.Should().ContainSingle().Which.Id.Should().Be(l2.Id);
        budget.Subtotal.Should().Be(100m);
        budget.Total.Should().Be(121m);
    }

    [Fact]
    public void RemoveLine_WithUnknownId_ThrowsInvalidOperation()
    {
        var budget = ValidBudget();

        var act = () => budget.RemoveLine(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangeStatus_UpdatesStatusAndStampsUpdater()
    {
        var budget = ValidBudget();
        var newStatus = Guid.NewGuid();
        var updaterId = Guid.NewGuid();

        budget.ChangeStatus(newStatus, updaterId);

        budget.StatusId.Should().Be(newStatus);
        budget.UpdatedByUserId.Should().Be(updaterId);
        budget.UpdatedAt.Should().NotBeNull();
    }
}
