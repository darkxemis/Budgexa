namespace Budgexa.Application.Customers.DTOs;

public sealed record CustomerDto(
    Guid Id,
    string LegalName,
    string? TradeName,
    string TaxId,
    string? Email,
    string? Phone,
    string? AddressLine,
    string? City,
    string? PostalCode,
    string? Province,
    string? Country,
    string? Notes,
    Guid CompanyId,
    string CompanyName,
    Guid StatusId,
    string StatusName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CustomerCreateDto(
    string LegalName,
    string? TradeName,
    string TaxId,
    string? Email,
    string? Phone,
    string? AddressLine,
    string? City,
    string? PostalCode,
    string? Province,
    string? Country,
    string? Notes);

public sealed record CustomerUpdateDto(
    string LegalName,
    string? TradeName,
    string TaxId,
    string? Email,
    string? Phone,
    string? AddressLine,
    string? City,
    string? PostalCode,
    string? Province,
    string? Country,
    string? Notes);

public sealed record CustomerGridDto(
    Guid Id,
    string LegalName,
    string? TradeName,
    string TaxId,
    string? Email,
    string? Phone,
    string? City,
    string? Country,
    Guid StatusId,
    string StatusName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
