namespace Budgexa.Application.Budgets.Services;

using Budgexa.Application.Budgets.DTOs;

public interface IAiService
{
    Task<BudgetItemsAiResult> GenerateBudgetJsonAsync(string userRequest, CancellationToken cancellationToken = default);
}
