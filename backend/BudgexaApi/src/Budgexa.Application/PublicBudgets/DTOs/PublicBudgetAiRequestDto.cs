namespace Budgexa.Application.PublicBudgets.DTOs;

public sealed record PublicBudgetAiRequestDto(
    Guid CompanyId,
    string UserRequest);
