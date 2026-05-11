namespace Budgexa.Application.Users.Commands.UpdateCurrentUser;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record UpdateCurrentUserCommand(UpdateCurrentUserDto Dto) : IRequest<UserProfileResult>;