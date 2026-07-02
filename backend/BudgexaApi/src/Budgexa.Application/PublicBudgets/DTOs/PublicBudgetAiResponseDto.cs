namespace Budgexa.Application.PublicBudgets.DTOs;

public sealed record PublicBudgetAiResponseDto(
    string OriginalRequest,
    List<PublicBudgetAiItemDto> Items,
    string Model);

public sealed record PublicBudgetAiItemDto(
    Guid ItemId,
    string ProductName,
    decimal UnitPrice,
    decimal TaxRate,
    string Unit,
    int Quantity);
