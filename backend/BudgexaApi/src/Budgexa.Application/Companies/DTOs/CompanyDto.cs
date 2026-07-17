namespace Budgexa.Application.Companies.DTOs;

public record CompanyDto(
    Guid Id,
    string Name,
    string? Description,
    string? Phone,
    string? Email,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateTime CreatedAt,
    Guid? CreatedByUserId,
    DateTime? UpdatedAt,
    Guid? UpdatedByUserId);