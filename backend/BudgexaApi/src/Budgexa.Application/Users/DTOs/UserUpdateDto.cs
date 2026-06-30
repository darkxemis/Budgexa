namespace Budgexa.Application.Users.DTOs;

public sealed record UserUpdateDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid LanguageId,
    List<Guid> RoleIds);