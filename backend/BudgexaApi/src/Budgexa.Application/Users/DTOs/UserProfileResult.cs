namespace Budgexa.Application.Users.DTOs;

public sealed record UserProfileResult(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid CompanyId,
    string CompanyName,
    Guid LanguageId,
    string Language,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
