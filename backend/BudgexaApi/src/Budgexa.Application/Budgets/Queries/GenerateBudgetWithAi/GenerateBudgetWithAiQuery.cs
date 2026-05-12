namespace Budgexa.Application.Budgets.Queries.GenerateBudgetWithAi;

using Budgexa.Application.Budgets.DTOs;
using MediatR;

public sealed record GenerateBudgetWithAiQuery(
    GenerateBudgetWithAiRequestDto Request
) : IRequest<BudgetItemsAiResponseDto>;