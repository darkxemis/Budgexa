namespace Budgexa.Application.Invoices.DTOs;

using Budgexa.Domain.Enums;

public sealed record InvoiceLineDto(
    Guid Id,
    Guid? ItemId,
    int SortOrder,
    string Description,
    string Unit,
    UnitMeasure UnitMeasure,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercentage,
    decimal TaxRate,
    decimal WithholdingRate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal WithholdingAmount,
    decimal Total);

public sealed record InvoiceLineUpsertDto(
    Guid? Id,
    Guid? ItemId,
    int SortOrder,
    string Description,
    string Unit,
    UnitMeasure UnitMeasure,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercentage,
    decimal TaxRate,
    decimal WithholdingRate);
