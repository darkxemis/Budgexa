namespace Budgexa.Domain.Entities;

using Budgexa.Domain.Common;
using Budgexa.Domain.Enums;

public sealed class Item : Entity
{
    public Guid CompanyId { get; private set; }
    public Guid StatusId { get; private set; }

    public string? Sku { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public ItemType Type { get; private set; }
    public string Unit { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public string Currency { get; private set; } = "EUR";

    public Company Company { get; private set; } = default!;
    public Status Status { get; private set; } = default!;

    private Item() { }

    private Item(
        Guid id,
        Guid companyId,
        Guid statusId,
        string? sku,
        string name,
        string? description,
        ItemType type,
        string unit,
        decimal unitPrice,
        decimal taxRate,
        string currency,
        Guid createdByUserId)
    {
        Id = id;
        CompanyId = companyId;
        StatusId = statusId;
        Sku = sku;
        Name = name;
        Description = description;
        Type = type;
        Unit = unit;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        Currency = currency;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
    }

    public static Item Create(
        Guid companyId,
        Guid statusId,
        string? sku,
        string name,
        string? description,
        ItemType type,
        string unit,
        decimal unitPrice,
        decimal taxRate,
        string currency,
        Guid createdByUserId,
        Guid? id = null)
    {
        Validate(name, unit, unitPrice, taxRate, currency);

        return new Item(
            id ?? Guid.NewGuid(),
            companyId,
            statusId,
            sku,
            name,
            description,
            type,
            unit,
            unitPrice,
            taxRate,
            currency,
            createdByUserId);
    }

    public void Update(
        string? sku,
        string name,
        string? description,
        ItemType type,
        string unit,
        decimal unitPrice,
        decimal taxRate,
        string currency,
        Guid updatedByUserId)
    {
        Validate(name, unit, unitPrice, taxRate, currency);

        Sku = sku;
        Name = name;
        Description = description;
        Type = type;
        Unit = unit;
        UnitPrice = unitPrice;
        TaxRate = taxRate;
        Currency = currency;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    public void MarkAsDeleted(Guid deletedStatusId, Guid updatedByUserId)
    {
        StatusId = deletedStatusId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByUserId = updatedByUserId;
    }

    private static void Validate(string name, string unit, decimal unitPrice, decimal taxRate, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Item unit cannot be empty.", nameof(unit));

        if (unitPrice < 0)
            throw new ArgumentException("Item unit price cannot be negative.", nameof(unitPrice));

        if (taxRate < 0 || taxRate > 100)
            throw new ArgumentException("Item tax rate must be between 0 and 100.", nameof(taxRate));

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
    }
}
