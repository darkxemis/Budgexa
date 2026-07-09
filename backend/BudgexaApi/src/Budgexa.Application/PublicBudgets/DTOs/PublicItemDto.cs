namespace Budgexa.Application.PublicBudgets.DTOs;

public sealed record PublicItemDto(
    Guid ItemId,
    string ProductName,
    decimal UnitPrice,
    decimal TaxRate,
    string Unit);
