namespace Budgexa.Application.Companies.DTOs;

public record CompanyUpdateDto(
    string Name,
    string? Description,
    string? Phone,
    string? Email,
    DateOnly? EndDate);