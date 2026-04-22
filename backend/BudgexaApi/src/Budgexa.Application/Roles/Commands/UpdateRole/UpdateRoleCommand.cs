using MediatR;

namespace Budgexa.Application.Roles.Commands.UpdateRole;

public record UpdateRoleCommand(Guid Id, string Name) : IRequest;