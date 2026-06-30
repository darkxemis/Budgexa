namespace Budgexa.Application.Budgets.DTOs;

public sealed record BudgetItem(
    string ProductName,
    int Quantity,
    string? Description = null);

public sealed record BudgetItemsAiResult(
    string OriginalRequest,
    List<BudgetItem> Items,
    string Model);