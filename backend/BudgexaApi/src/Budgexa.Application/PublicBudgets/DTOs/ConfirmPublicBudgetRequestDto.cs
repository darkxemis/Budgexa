namespace Budgexa.Application.PublicBudgets.DTOs;

public sealed record ConfirmPublicBudgetRequestDto(
    Guid CompanyId,
    string CustomerFirstName,
    string CustomerLastName,
    string? CustomerPhone,
    string? CustomerEmail,
    List<ConfirmPublicBudgetLineDto> Lines);

public sealed record ConfirmPublicBudgetLineDto(
    Guid ItemId,
    decimal Quantity);
