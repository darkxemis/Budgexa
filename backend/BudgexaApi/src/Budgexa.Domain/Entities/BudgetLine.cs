namespace Budgexa.Domain.Entities;

public sealed class BudgetLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BudgetId { get; private set; }
    public Guid? ItemId { get; private set; }

    public int SortOrder { get; private set; }
    public string Description { get; private set; } = default!;
    public string Unit { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal TaxRate { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }

    public Budget Budget { get; private set; } = default!;
    public Item? Item { get; private set; }

    private BudgetLine() { }

    private BudgetLine(
        Guid id,
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate)
    {
        Id = id;
        ItemId = itemId;
        SortOrder = sortOrder;
        Description = description;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = discountPercentage;
        TaxRate = taxRate;
        Recalculate();
    }

    internal static BudgetLine Create(
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        Guid? id = null)
    {
        Validate(description, unit, quantity, unitPrice, discountPercentage, taxRate);

        return new BudgetLine(
            id ?? Guid.NewGuid(),
            itemId,
            sortOrder,
            description,
            unit,
            quantity,
            unitPrice,
            discountPercentage,
            taxRate);
    }

    internal void Update(
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate)
    {
        Validate(description, unit, quantity, unitPrice, discountPercentage, taxRate);

        ItemId = itemId;
        SortOrder = sortOrder;
        Description = description;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = discountPercentage;
        TaxRate = taxRate;
        Recalculate();
    }

    internal void AttachTo(Guid budgetId) => BudgetId = budgetId;

    private void Recalculate()
    {
        var gross = Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
        var discount = Math.Round(gross * (DiscountPercentage / 100m), 2, MidpointRounding.AwayFromZero);
        Subtotal = gross - discount;
        TaxAmount = Math.Round(Subtotal * (TaxRate / 100m), 2, MidpointRounding.AwayFromZero);
        Total = Subtotal + TaxAmount;
    }

    private static void Validate(
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Budget line description cannot be empty.", nameof(description));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Budget line unit cannot be empty.", nameof(unit));

        if (quantity <= 0)
            throw new ArgumentException("Budget line quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Budget line unit price cannot be negative.", nameof(unitPrice));

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Budget line discount must be between 0 and 100.", nameof(discountPercentage));

        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Budget line tax rate must be between 0 and 100.", nameof(taxRate));
    }
}
