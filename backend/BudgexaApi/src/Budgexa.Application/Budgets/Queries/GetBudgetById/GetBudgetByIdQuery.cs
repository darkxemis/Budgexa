namespace Budgexa.Application.Budgets.Queries.GetBudgetById;

using Budgexa.Application.Budgets.DTOs;
using MediatR;

public sealed record GetBudgetByIdQuery(Guid Id) : IRequest<BudgetDto>;
