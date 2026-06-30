namespace Budgexa.Application.Users.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    CompanyInfo Company,
    LanguageInfo Language,
    List<RoleInfo> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CompanyInfo(Guid Id, string Name);
public sealed record LanguageInfo(Guid Id, string Name);
public sealed record RoleInfo(Guid Id, string Name);