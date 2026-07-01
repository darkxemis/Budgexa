namespace Budgexa.Application.Items.Queries.GetItemById;

using Budgexa.Application.Items.DTOs;
using MediatR;

public sealed record GetItemByIdQuery(Guid Id) : IRequest<ItemDto>;
