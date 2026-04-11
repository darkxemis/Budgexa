namespace Budgexa.Application.Auth.Queries.Login;

using MediatR;
using Budgexa.Application.Auth.DTOs;

public sealed record LoginQuery(string Email, string Password) : IRequest<AuthResult>;
