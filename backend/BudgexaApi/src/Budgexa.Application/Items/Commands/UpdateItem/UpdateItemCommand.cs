namespace Budgexa.Application.Items.Commands.UpdateItem;

using Budgexa.Application.Items.DTOs;
using MediatR;

public sealed record UpdateItemCommand(Guid Id, ItemUpdateDto Dto) : IRequest<ItemDto>;
