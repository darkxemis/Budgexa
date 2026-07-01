namespace Budgexa.Application.Budgets.Commands.UpdateBudget;

using Budgexa.Application.Budgets.DTOs;
using MediatR;

public sealed record UpdateBudgetCommand(Guid Id, BudgetUpdateDto Dto) : IRequest<BudgetDto>;
