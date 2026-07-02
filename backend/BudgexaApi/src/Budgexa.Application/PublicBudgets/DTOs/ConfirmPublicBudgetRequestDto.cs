namespace Budgexa.Application.PublicBudgets.DTOs;

public sealed record ConfirmPublicBudgetRequestDto(
    Guid CompanyId,
    string CustomerFirstName,
    string CustomerLastName,
    List<ConfirmPublicBudgetLineDto> Lines);

public sealed record ConfirmPublicBudgetLineDto(
    Guid ItemId,
    decimal Quantity);
