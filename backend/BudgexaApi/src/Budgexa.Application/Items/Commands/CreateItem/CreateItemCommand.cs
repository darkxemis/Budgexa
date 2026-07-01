namespace Budgexa.Application.Items.Commands.CreateItem;

using Budgexa.Application.Items.DTOs;
using MediatR;

public sealed record CreateItemCommand(ItemCreateDto Dto) : IRequest<ItemDto>;
