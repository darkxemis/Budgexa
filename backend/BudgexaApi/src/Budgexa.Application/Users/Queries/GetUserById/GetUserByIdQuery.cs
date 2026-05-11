namespace Budgexa.Application.Users.Queries.GetUserById;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserProfileResult?>;