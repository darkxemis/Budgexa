namespace Budgexa.Application.Budgets.Queries.GetAllBudgets;

using Budgexa.Application.Budgets.DTOs;
using MediatR;

public sealed record GetAllBudgetsQuery() : IRequest<IEnumerable<BudgetGridDto>>;
