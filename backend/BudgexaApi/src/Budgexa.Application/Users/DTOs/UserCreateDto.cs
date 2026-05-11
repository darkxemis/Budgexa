namespace Budgexa.Application.Users.DTOs;

public sealed record UserCreateDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid CompanyId,
    Guid LanguageId,
    List<Guid> RoleIds);