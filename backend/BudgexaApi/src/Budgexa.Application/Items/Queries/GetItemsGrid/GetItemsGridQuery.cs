namespace Budgexa.Application.Items.Queries.GetItemsGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Items.DTOs;
using MediatR;

public sealed record GetItemsGridQuery(GridRequestDto Request) : IRequest<GridResponseDto<ItemGridDto>>;
