namespace Budgexa.Application.Budgets.Queries.GenerateBudgetWithAi;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Services;
using MediatR;

public sealed class GenerateBudgetWithAiQueryHandler(
    IAiService aiService)
    : IRequestHandler<GenerateBudgetWithAiQuery, BudgetItemsAiResponseDto>
{
    public async Task<BudgetItemsAiResponseDto> Handle(
        GenerateBudgetWithAiQuery request,
        CancellationToken cancellationToken)
    {
        var result = await aiService.GenerateBudgetJsonAsync(request.Request.UserRequest, cancellationToken);

        return new BudgetItemsAiResponseDto(
            result.OriginalRequest,
            result.Items,
            result.Model
        );
    }
}