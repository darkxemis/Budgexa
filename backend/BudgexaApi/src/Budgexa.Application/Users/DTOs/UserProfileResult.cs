namespace Budgexa.Application.Users.DTOs;

public sealed record UserProfileResult(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt);
