namespace Budgexa.Domain.Entities;

public sealed class InvoiceLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid InvoiceId { get; private set; }
    public Guid? ItemId { get; private set; }

    public int SortOrder { get; private set; }
    public string Description { get; private set; } = default!;
    public string Unit { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal WithholdingRate { get; private set; }

    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal WithholdingAmount { get; private set; }
    public decimal Total { get; private set; }

    public Invoice Invoice { get; private set; } = default!;
    public Item? Item { get; private set; }

    private InvoiceLine() { }

    private InvoiceLine(
        Guid id,
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        decimal withholdingRate)
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
        WithholdingRate = withholdingRate;
        Recalculate();
    }

    internal static InvoiceLine Create(
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        decimal withholdingRate,
        Guid? id = null)
    {
        Validate(description, unit, quantity, unitPrice, discountPercentage, taxRate, withholdingRate);

        return new InvoiceLine(
            id ?? Guid.NewGuid(),
            itemId,
            sortOrder,
            description,
            unit,
            quantity,
            unitPrice,
            discountPercentage,
            taxRate,
            withholdingRate);
    }

    internal void Update(
        Guid? itemId,
        int sortOrder,
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        decimal withholdingRate)
    {
        Validate(description, unit, quantity, unitPrice, discountPercentage, taxRate, withholdingRate);

        ItemId = itemId;
        SortOrder = sortOrder;
        Description = description;
        Unit = unit;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercentage = discountPercentage;
        TaxRate = taxRate;
        WithholdingRate = withholdingRate;
        Recalculate();
    }

    internal void AttachTo(Guid invoiceId) => InvoiceId = invoiceId;

    private void Recalculate()
    {
        var gross = Math.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);
        var discount = Math.Round(gross * (DiscountPercentage / 100m), 2, MidpointRounding.AwayFromZero);
        Subtotal = gross - discount;
        TaxAmount = Math.Round(Subtotal * (TaxRate / 100m), 2, MidpointRounding.AwayFromZero);
        WithholdingAmount = Math.Round(Subtotal * (WithholdingRate / 100m), 2, MidpointRounding.AwayFromZero);
        Total = Subtotal + TaxAmount - WithholdingAmount;
    }

    private static void Validate(
        string description,
        string unit,
        decimal quantity,
        decimal unitPrice,
        decimal discountPercentage,
        decimal taxRate,
        decimal withholdingRate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Invoice line description cannot be empty.", nameof(description));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Invoice line unit cannot be empty.", nameof(unit));

        if (quantity <= 0)
            throw new ArgumentException("Invoice line quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Invoice line unit price cannot be negative.", nameof(unitPrice));

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Invoice line discount must be between 0 and 100.", nameof(discountPercentage));

        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Invoice line tax rate must be between 0 and 100.", nameof(taxRate));

        if (withholdingRate < 0 || withholdingRate > 100)
            throw new ArgumentException("Invoice line withholding rate must be between 0 and 100.", nameof(withholdingRate));
    }
}
