namespace Budgexa.Domain.Tests.Entities;

using Budgexa.Domain.Entities;

public class BudgetLineTests
{
    private static Budget BudgetWithLine(
        out BudgetLine line,
        decimal quantity = 1m,
        decimal unitPrice = 100m,
        decimal discount = 0m,
        decimal taxRate = 21m)
    {
        var budget = Budget.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "BUD-1",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            "EUR",
            null,
            null,
            Guid.NewGuid());

        line = budget.AddLine(null, 1, "Service", "unit", quantity, unitPrice, discount, taxRate, Guid.NewGuid());
        return budget;
    }

    [Fact]
    public void Create_WithValidArguments_CalculatesTotals()
    {
        BudgetWithLine(out var line, quantity: 2m, unitPrice: 50m, discount: 0m, taxRate: 21m);

        line.Quantity.Should().Be(2m);
        line.UnitPrice.Should().Be(50m);
        line.DiscountPercentage.Should().Be(0m);
        line.TaxRate.Should().Be(21m);
        line.Subtotal.Should().Be(100m);
        line.TaxAmount.Should().Be(21m);
        line.Total.Should().Be(121m);
    }

    [Fact]
    public void Create_WithDiscount_AppliesDiscountBeforeTax()
    {
        BudgetWithLine(out var line, quantity: 1m, unitPrice: 100m, discount: 25m, taxRate: 10m);

        line.Subtotal.Should().Be(75m);
        line.TaxAmount.Should().Be(7.50m);
        line.Total.Should().Be(82.50m);
    }

    [Fact]
    public void Create_WithFullDiscount_ResultsInZeroTotals()
    {
        BudgetWithLine(out var line, quantity: 5m, unitPrice: 10m, discount: 100m, taxRate: 21m);

        line.Subtotal.Should().Be(0m);
        line.TaxAmount.Should().Be(0m);
        line.Total.Should().Be(0m);
    }

    [Fact]
    public void Update_RecalculatesTotals()
    {
        var budget = BudgetWithLine(out var line);

        budget.UpdateLine(line.Id, null, 1, "X", "unit", 3m, 50m, 0m, 21m, Guid.NewGuid());

        line.Quantity.Should().Be(3m);
        line.Subtotal.Should().Be(150m);
        line.TaxAmount.Should().Be(31.50m);
        line.Total.Should().Be(181.50m);
    }

    [Fact]
    public void Create_AttachesToBudget()
    {
        var budget = BudgetWithLine(out var line);

        line.BudgetId.Should().Be(budget.Id);
    }

    [Theory]
    [InlineData("", "unit", 1, 1, 0, 0)]
    [InlineData("desc", "", 1, 1, 0, 0)]
    [InlineData("desc", "unit", 0, 1, 0, 0)]
    [InlineData("desc", "unit", -1, 1, 0, 0)]
    [InlineData("desc", "unit", 1, -1, 0, 0)]
    [InlineData("desc", "unit", 1, 1, -1, 0)]
    [InlineData("desc", "unit", 1, 1, 101, 0)]
    [InlineData("desc", "unit", 1, 1, 0, -1)]
    [InlineData("desc", "unit", 1, 1, 0, 101)]
    public void AddLine_WithInvalidArguments_ThrowsArgumentException(
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discount,
        decimal taxRate)
    {
        var budget = Budget.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "BUD-1", DateOnly.FromDateTime(DateTime.UtcNow), null,
            "EUR", null, null, Guid.NewGuid());

        var act = () => budget.AddLine(null, 1, description, unit, quantity, unitPrice, discount, taxRate, Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }
}
