namespace Budgexa.Application.Auth.Commands.Register;

using MediatR;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    Guid[] RoleIds,
    string LastName) : IRequest<Guid>;
