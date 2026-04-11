namespace Budgexa.Application.Users.Queries.GetCurrentUser;

using MediatR;
using Budgexa.Application.Users.DTOs;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<UserProfileResult>;
