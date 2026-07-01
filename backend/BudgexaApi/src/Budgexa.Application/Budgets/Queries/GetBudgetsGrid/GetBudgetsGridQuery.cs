namespace Budgexa.Application.Budgets.Queries.GetBudgetsGrid;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GetBudgetsGridQuery(GridRequestDto Request) : IRequest<GridResponseDto<BudgetGridDto>>;
