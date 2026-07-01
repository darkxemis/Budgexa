namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;

public class InvoiceTests
{
    private static Invoice ValidInvoice(Guid? budgetId = null) =>
        Invoice.Create(
            companyId: Guid.NewGuid(),
            customerId: Guid.NewGuid(),
            statusId: Guid.NewGuid(),
            series: "A",
            number: "INV-0001",
            issueDate: DateOnly.FromDateTime(DateTime.UtcNow),
            dueDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            currency: "EUR",
            notes: "Notes",
            createdByUserId: Guid.NewGuid(),
            budgetId: budgetId);

    [Fact]
    public void Create_WithValidArguments_InitializesProperties()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var statusId = Guid.NewGuid();
        var budgetId = Guid.NewGuid();
        var creator = Guid.NewGuid();
        var issue = DateOnly.FromDateTime(DateTime.UtcNow);
        var due = issue.AddDays(30);

        var invoice = Invoice.Create(
            companyId,
            customerId,
            statusId,
            "A",
            "INV-0001",
            issue,
            due,
            "EUR",
            "Some notes",
            creator,
            budgetId);

        invoice.Id.Should().NotBe(Guid.Empty);
        invoice.CompanyId.Should().Be(companyId);
        invoice.CustomerId.Should().Be(customerId);
        invoice.StatusId.Should().Be(statusId);
        invoice.BudgetId.Should().Be(budgetId);
        invoice.Series.Should().Be("A");
        invoice.Number.Should().Be("INV-0001");
        invoice.IssueDate.Should().Be(issue);
        invoice.DueDate.Should().Be(due);
        invoice.Currency.Should().Be("EUR");
        invoice.Notes.Should().Be("Some notes");
        invoice.CreatedByUserId.Should().Be(creator);
        invoice.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        invoice.Lines.Should().BeEmpty();
        invoice.Subtotal.Should().Be(0);
        invoice.TaxAmount.Should().Be(0);
        invoice.WithholdingAmount.Should().Be(0);
        invoice.Total.Should().Be(0);
        invoice.AmountPaid.Should().Be(0);
        invoice.AmountDue.Should().Be(0);
        invoice.IsFullyPaid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "INV-0001")]
    [InlineData("   ", "INV-0001")]
    [InlineData(null, "INV-0001")]
    [InlineData("A", "")]
    [InlineData("A", "   ")]
    [InlineData("A", null)]
    public void Create_WithBlankSeriesOrNumber_ThrowsArgumentException(string? series, string? number)
    {
        var act = () => Invoice.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            series!, number!,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            "EUR", null, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    public void Create_WithInvalidCurrency_ThrowsArgumentException(string currency)
    {
        var act = () => Invoice.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "A", "INV-0001",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            currency, null, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithDueDateBeforeIssueDate_ThrowsArgumentException()
    {
        var issue = DateOnly.FromDateTime(DateTime.UtcNow);

        var act = () => Invoice.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "A", "INV-0001",
            issue,
            issue.AddDays(-1),
            "EUR", null, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateHeader_WithValidArguments_ReplacesFields()
    {
        var invoice = ValidInvoice();
        var newCustomer = Guid.NewGuid();
        var newIssue = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        invoice.UpdateHeader(
            newCustomer,
            "B",
            "INV-0002",
            newIssue,
            newIssue.AddDays(45),
            "USD",
            "Updated",
            Guid.NewGuid());

        invoice.CustomerId.Should().Be(newCustomer);
        invoice.Series.Should().Be("B");
        invoice.Number.Should().Be("INV-0002");
        invoice.IssueDate.Should().Be(newIssue);
        invoice.DueDate.Should().Be(newIssue.AddDays(45));
        invoice.Currency.Should().Be("USD");
        invoice.Notes.Should().Be("Updated");
        invoice.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AddLine_RecalculatesTotalsWithWithholding()
    {
        var invoice = ValidInvoice();

        invoice.AddLine(
            itemId: null,
            sortOrder: 1,
            description: "Service",
            unit: "hour",
            quantity: 2m,
            unitPrice: 100m,
            discountPercentage: 0m,
            taxRate: 21m,
            withholdingRate: 15m,
            updatedByUserId: Guid.NewGuid());

        invoice.Subtotal.Should().Be(200m);
        invoice.TaxAmount.Should().Be(42m);
        invoice.WithholdingAmount.Should().Be(30m);
        invoice.Total.Should().Be(212m);
        invoice.AmountDue.Should().Be(212m);
        invoice.IsFullyPaid.Should().BeFalse();
    }

    [Fact]
    public void UpdateLine_RecalculatesTotals()
    {
        var invoice = ValidInvoice();
        var line = invoice.AddLine(null, 1, "Service", "hour", 1m, 100m, 0m, 21m, 0m, Guid.NewGuid());

        invoice.UpdateLine(line.Id, null, 1, "Updated", "hour", 3m, 50m, 0m, 21m, 0m, Guid.NewGuid());

        invoice.Subtotal.Should().Be(150m);
        invoice.TaxAmount.Should().Be(31.50m);
        invoice.Total.Should().Be(181.50m);
    }

    [Fact]
    public void UpdateLine_WithUnknownId_ThrowsInvalidOperation()
    {
        var invoice = ValidInvoice();

        var act = () => invoice.UpdateLine(Guid.NewGuid(), null, 1, "x", "u", 1m, 1m, 0m, 0m, 0m, Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveLine_RemovesAndRecalculates()
    {
        var invoice = ValidInvoice();
        var l1 = invoice.AddLine(null, 1, "A", "u", 1m, 100m, 0m, 21m, 0m, Guid.NewGuid());
        var l2 = invoice.AddLine(null, 2, "B", "u", 1m, 50m, 0m, 21m, 0m, Guid.NewGuid());

        invoice.RemoveLine(l1.Id, Guid.NewGuid());

        invoice.Lines.Should().ContainSingle().Which.Id.Should().Be(l2.Id);
        invoice.Subtotal.Should().Be(50m);
        invoice.Total.Should().Be(60.50m);
    }

    [Fact]
    public void RemoveLine_WithUnknownId_ThrowsInvalidOperation()
    {
        var invoice = ValidInvoice();

        var act = () => invoice.RemoveLine(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RegisterPayment_AccumulatesAmountAndMarksPartial()
    {
        var invoice = ValidInvoice();
        invoice.AddLine(null, 1, "S", "u", 1m, 100m, 0m, 21m, 0m, Guid.NewGuid());

        invoice.RegisterPayment(50m, PaymentMethod.BankTransfer, "REF-1", Guid.NewGuid());

        invoice.AmountPaid.Should().Be(50m);
        invoice.AmountDue.Should().Be(71m);
        invoice.PaymentMethod.Should().Be(PaymentMethod.BankTransfer);
        invoice.PaymentReference.Should().Be("REF-1");
        invoice.IsFullyPaid.Should().BeFalse();
    }

    [Fact]
    public void RegisterPayment_WhenAmountCompletesTotal_MarksFullyPaid()
    {
        var invoice = ValidInvoice();
        invoice.AddLine(null, 1, "S", "u", 1m, 100m, 0m, 21m, 0m, Guid.NewGuid());

        invoice.RegisterPayment(121m, PaymentMethod.Cash, null, Guid.NewGuid());

        invoice.IsFullyPaid.Should().BeTrue();
        invoice.AmountDue.Should().Be(0m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegisterPayment_WithNonPositiveAmount_ThrowsArgumentException(decimal amount)
    {
        var invoice = ValidInvoice();
        invoice.AddLine(null, 1, "S", "u", 1m, 100m, 0m, 21m, 0m, Guid.NewGuid());

        var act = () => invoice.RegisterPayment(amount, PaymentMethod.Cash, null, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegisterPayment_WhenExceedsTotal_ThrowsInvalidOperation()
    {
        var invoice = ValidInvoice();
        invoice.AddLine(null, 1, "S", "u", 1m, 100m, 0m, 21m, 0m, Guid.NewGuid());

        var act = () => invoice.RegisterPayment(200m, PaymentMethod.Cash, null, Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ChangeStatus_UpdatesStatusAndStampsUpdater()
    {
        var invoice = ValidInvoice();
        var newStatus = Guid.NewGuid();
        var updaterId = Guid.NewGuid();

        invoice.ChangeStatus(newStatus, updaterId);

        invoice.StatusId.Should().Be(newStatus);
        invoice.UpdatedByUserId.Should().Be(updaterId);
        invoice.UpdatedAt.Should().NotBeNull();
    }
}
