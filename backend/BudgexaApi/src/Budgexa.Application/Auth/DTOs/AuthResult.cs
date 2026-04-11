namespace Budgexa.Application.Auth.DTOs;

public sealed record AuthResult(string Token, string Email, string FullName);
