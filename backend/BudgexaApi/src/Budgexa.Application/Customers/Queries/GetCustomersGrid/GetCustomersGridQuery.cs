namespace Budgexa.Application.Customers.Queries.GetCustomersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Customers.DTOs;
using MediatR;

public sealed record GetCustomersGridQuery(GridRequestDto Request) : IRequest<GridResponseDto<CustomerGridDto>>;
