namespace Budgexa.Application.Users.Commands.CreateUser;

using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record CreateUserCommand(UserCreateDto Dto) : IRequest<UserProfileResult>;