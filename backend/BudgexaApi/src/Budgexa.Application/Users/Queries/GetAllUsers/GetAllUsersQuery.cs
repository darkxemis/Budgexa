namespace Budgexa.Application.Users.Queries.GetAllUsers;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record GetAllUsersQuery : IRequest<IEnumerable<UserDto>>;