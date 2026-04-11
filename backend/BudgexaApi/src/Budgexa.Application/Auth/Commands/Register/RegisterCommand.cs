namespace Budgexa.Application.Auth.Commands.Register;

using MediatR;
using Budgexa.Application.Auth.DTOs;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<AuthResult>;
