namespace Budgexa.Application.Budgets.DTOs;

public sealed record BudgetLineDto(
    Guid Id,
    Guid? ItemId,
    int SortOrder,
    string Description,
    string Unit,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercentage,
    decimal TaxRate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total);

public sealed record BudgetLineUpsertDto(
    Guid? Id,
    Guid? ItemId,
    int SortOrder,
    string Description,
    string Unit,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercentage,
    decimal TaxRate);
