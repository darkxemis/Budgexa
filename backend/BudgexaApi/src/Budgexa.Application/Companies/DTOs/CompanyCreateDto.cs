namespace Budgexa.Application.Companies.DTOs;

public record CompanyCreateDto(
    string Name,
    string? Description,
    string? Phone,
    string? Email,
    DateOnly StartDate,
    DateOnly? EndDate);