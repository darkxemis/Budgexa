namespace Budgexa.Application.Users.DTOs;

public sealed record UpdateCurrentUserDto(
    string FirstName,
    string LastName,
    string Password,
    Guid LanguageId);