namespace Budgexa.Application.Budgets.Commands.ChangeBudgetStatus;

using Budgexa.Application.Budgets.DTOs;
using MediatR;

public sealed record ChangeBudgetStatusCommand(Guid Id, ChangeBudgetStatusDto Dto) : IRequest<BudgetDto>;
