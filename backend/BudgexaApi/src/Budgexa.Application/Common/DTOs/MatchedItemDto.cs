namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// Matched item with resolved DB fields.
/// </summary>
public sealed record MatchedItemDto(
    Guid? ItemId,
    string ProductName,
    decimal Quantity,
    decimal? DiscountPercentage,
    decimal? UnitPrice,
    decimal? TaxRate,
    string? Unit,
    int? UnitMeasure // Enum as int for API serialization
);
