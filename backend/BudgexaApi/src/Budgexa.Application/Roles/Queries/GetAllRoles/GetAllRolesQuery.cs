using Budgexa.Application.Roles.DTOs;
using MediatR;

public record GetAllRolesQuery() : IRequest<List<RoleDto>>;
