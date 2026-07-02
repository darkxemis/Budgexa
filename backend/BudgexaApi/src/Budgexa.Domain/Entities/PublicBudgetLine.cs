namespace Budgexa.Domain.Entities;

public sealed class PublicBudgetLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PublicBudgetId { get; private set; }
    public Guid ItemId { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; }
    public decimal TaxPercentage { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal SubTotal { get; private set; }

    public PublicBudget PublicBudget { get; private set; } = default!;
    public Item Item { get; private set; } = default!;

    private PublicBudgetLine() { }

    private PublicBudgetLine(
        Guid id,
        Guid itemId,
        string description,
        decimal quantity,
        decimal price,
        decimal taxPercentage)
    {
        Id = id;
        ItemId = itemId;
        Description = description;
        Quantity = quantity;
        Price = price;
        TaxPercentage = taxPercentage;
        Recalculate();
    }

    public static PublicBudgetLine Create(
        Guid itemId,
        string description,
        decimal quantity,
        decimal price,
        decimal taxPercentage,
        Guid? id = null)
    {
        Validate(description, quantity, price, taxPercentage);

        return new PublicBudgetLine(
            id ?? Guid.NewGuid(),
            itemId,
            description,
            quantity,
            price,
            taxPercentage);
    }

    internal void AttachTo(Guid publicBudgetId) => PublicBudgetId = publicBudgetId;

    private void Recalculate()
    {
        SubTotal = Math.Round(Quantity * Price, 2, MidpointRounding.AwayFromZero);
        TaxAmount = Math.Round(SubTotal * (TaxPercentage / 100m), 2, MidpointRounding.AwayFromZero);
    }

    private static void Validate(string description, decimal quantity, decimal price, decimal taxPercentage)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Public budget line description cannot be empty.", nameof(description));

        if (quantity <= 0)
            throw new ArgumentException("Public budget line quantity must be greater than zero.", nameof(quantity));

        if (price < 0)
            throw new ArgumentException("Public budget line price cannot be negative.", nameof(price));

        if (taxPercentage < 0 || taxPercentage > 100)
            throw new ArgumentException("Public budget line tax percentage must be between 0 and 100.", nameof(taxPercentage));
    }
}
