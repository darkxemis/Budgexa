namespace Budgexa.Application.Users.Commands.DeleteUser;

using MediatR;

public sealed record DeleteUserCommand(Guid Id) : IRequest;