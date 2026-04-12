namespace Budgexa.Application.Auth.DTOs;

public sealed record AuthResult(Guid UserId, string Token, string RefreshToken, string Email, string FullName);
