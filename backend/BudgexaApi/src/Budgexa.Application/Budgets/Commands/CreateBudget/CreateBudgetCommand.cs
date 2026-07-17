namespace Budgexa.Application.Budgets.Commands.CreateBudget;

using Budgexa.Application.Budgets.DTOs;
using MediatR;

public sealed record CreateBudgetCommand(BudgetCreateDto Dto) : IRequest<BudgetDto>;
