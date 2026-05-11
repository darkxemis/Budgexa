namespace Budgexa.Application.Users.Commands.UpdateUser;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record UpdateUserCommand(Guid Id, UserUpdateDto Dto) : IRequest<UserProfileResult?>;