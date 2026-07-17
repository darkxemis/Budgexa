namespace Budgexa.Application.PublicBudgets.Queries.GeneratePublicBudgetWithAi;

using Budgexa.Application.PublicBudgets.DTOs;
using MediatR;

public sealed record GeneratePublicBudgetWithAiQuery(
    PublicBudgetAiRequestDto Request
) : IRequest<PublicBudgetAiResponseDto>;
