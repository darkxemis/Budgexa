namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// Raw AI extraction for private budget (before matching/parsing).
/// All fields nullable - maps to PrivateBudgetPrompt JSON.
/// </summary>
public sealed record AiPrivateBudgetDto(
    string? CustomerName,
    string? CustomerTaxId,
    string? Number,
    string? IssueDate,
    string? ValidUntil,
    string? Currency,
    string? Notes,
    string? TermsAndConditions,
    List<AiItemDto>? Items
);
