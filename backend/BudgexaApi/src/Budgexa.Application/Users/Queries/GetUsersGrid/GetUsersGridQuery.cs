namespace Budgexa.Application.Users.Queries.GetUsersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Users.DTOs;
using MediatR;

public sealed record GetUsersGridQuery(GridRequestDto Request) : IRequest<GridResponseDto<UserGridDto>>;
