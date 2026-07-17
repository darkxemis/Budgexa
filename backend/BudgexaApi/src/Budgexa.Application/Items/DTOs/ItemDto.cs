namespace Budgexa.Application.Items.DTOs;

using Budgexa.Domain.Enums;

public sealed record ItemDto(
    Guid Id,
    string? Sku,
    string Name,
    string? Description,
    ItemType Type,
    string Unit,
    decimal UnitPrice,
    decimal TaxRate,
    string Currency,
    Guid CompanyId,
    string CompanyName,
    Guid StatusId,
    string StatusName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ItemCreateDto(
    string? Sku,
    string Name,
    string? Description,
    ItemType Type,
    string Unit,
    decimal UnitPrice,
    decimal TaxRate,
    string Currency);

public sealed record ItemUpdateDto(
    string? Sku,
    string Name,
    string? Description,
    ItemType Type,
    string Unit,
    decimal UnitPrice,
    decimal TaxRate,
    string Currency);

public sealed record ItemGridDto(
    Guid Id,
    string? Sku,
    string Name,
    string? Description,
    ItemType Type,
    string Unit,
    decimal UnitPrice,
    decimal TaxRate,
    string Currency,
    Guid StatusId,
    string StatusName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
