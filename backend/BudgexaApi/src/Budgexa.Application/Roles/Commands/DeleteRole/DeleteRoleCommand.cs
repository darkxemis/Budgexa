using MediatR;

namespace Budgexa.Application.Roles.Commands.DeleteRole;

public record DeleteRoleCommand(Guid Id) : IRequest;