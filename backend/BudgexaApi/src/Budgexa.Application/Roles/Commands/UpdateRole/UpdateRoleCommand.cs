using Budgexa.Application.Roles.DTOs;
using MediatR;

namespace Budgexa.Application.Roles.Commands.UpdateRole;

public sealed record UpdateRoleCommand(Guid Id, UpdateRoleDto Dto) : IRequest;