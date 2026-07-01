namespace Budgexa.Application.Budgets.Commands.DeleteBudget;

using MediatR;

public sealed record DeleteBudgetCommand(Guid Id) : IRequest;
