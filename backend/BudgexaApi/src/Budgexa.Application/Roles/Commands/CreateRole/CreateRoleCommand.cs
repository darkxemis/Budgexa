namespace Budgexa.Application.Roles.Commands.CreateRole;

using MediatR;

public record CreateRoleCommand(string Name) : IRequest<Guid>;