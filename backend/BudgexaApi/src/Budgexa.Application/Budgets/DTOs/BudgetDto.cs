namespace Budgexa.Application.Budgets.DTOs;

public sealed record BudgetDto(
    Guid Id,
    string Number,
    DateOnly IssueDate,
    DateOnly? ValidUntil,
    string Currency,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    string? Notes,
    string? TermsAndConditions,
    Guid CompanyId,
    string CompanyName,
    Guid CustomerId,
    string CustomerName,
    Guid StatusId,
    string StatusName,
    List<BudgetLineDto> Lines,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record BudgetCreateDto(
    Guid CustomerId,
    string Number,
    DateOnly IssueDate,
    DateOnly? ValidUntil,
    string Currency,
    string? Notes,
    string? TermsAndConditions,
    List<BudgetLineUpsertDto> Lines);

public sealed record BudgetUpdateDto(
    Guid CustomerId,
    string Number,
    DateOnly IssueDate,
    DateOnly? ValidUntil,
    string Currency,
    string? Notes,
    string? TermsAndConditions,
    List<BudgetLineUpsertDto> Lines);

public sealed record BudgetGridDto(
    Guid Id,
    string Number,
    DateOnly IssueDate,
    DateOnly? ValidUntil,
    string Currency,
    decimal Total,
    Guid CustomerId,
    string CustomerName,
    Guid StatusId,
    string StatusName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ChangeBudgetStatusDto(Guid StatusId);
