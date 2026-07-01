namespace Budgexa.Application.Items.Commands.DeleteItem;

using MediatR;

public sealed record DeleteItemCommand(Guid Id) : IRequest;
