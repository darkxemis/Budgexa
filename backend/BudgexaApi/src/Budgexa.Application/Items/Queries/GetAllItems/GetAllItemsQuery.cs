namespace Budgexa.Application.Items.Queries.GetAllItems;

using Budgexa.Application.Items.DTOs;
using MediatR;

public sealed record GetAllItemsQuery() : IRequest<IEnumerable<ItemDto>>;
