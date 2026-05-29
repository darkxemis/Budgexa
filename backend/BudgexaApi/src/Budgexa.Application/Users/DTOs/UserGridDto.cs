namespace Budgexa.Application.Users.DTOs;

public sealed record UserGridDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    Guid CompanyId,
    string CompanyName,
    Guid LanguageId,
    string LanguageName,
    Guid StatusId,
    string StatusName,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<RoleInfo> Roles);
