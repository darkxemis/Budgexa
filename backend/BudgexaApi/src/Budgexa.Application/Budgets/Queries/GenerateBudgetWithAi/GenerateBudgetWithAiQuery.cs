namespace Budgexa.Application.Budgets.Queries.GenerateBudgetWithAi;

using Budgexa.Application.Common.DTOs;
using MediatR;

public sealed record GenerateBudgetWithAiQuery(
    PrivateBudgetAiRequestDto Request
) : IRequest<PrivateBudgetAiResponseDto>;
