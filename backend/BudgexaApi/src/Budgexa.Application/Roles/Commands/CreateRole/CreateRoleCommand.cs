namespace Budgexa.Application.Roles.Commands.CreateRole;

using Budgexa.Application.Roles.DTOs;
using MediatR;

public sealed record CreateRoleCommand(RoleCreateDto Dto) : IRequest<Guid>;