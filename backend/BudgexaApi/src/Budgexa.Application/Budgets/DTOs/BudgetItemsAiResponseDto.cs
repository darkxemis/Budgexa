namespace Budgexa.Application.Budgets.DTOs;

public sealed record BudgetItemsAiResponseDto(
    string OriginalRequest,
    List<BudgetItem> Items,
    string Model
);