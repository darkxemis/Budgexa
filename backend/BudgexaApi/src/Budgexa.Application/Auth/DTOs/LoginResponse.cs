namespace Budgexa.Application.Auth.DTOs;

public sealed record LoginResponse(Guid UserId, string Email, string FullName);
