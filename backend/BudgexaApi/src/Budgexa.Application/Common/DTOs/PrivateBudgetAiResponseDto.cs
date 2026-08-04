namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// Response DTO for authenticated budget generation with AI.
/// Contains only essential fields needed by the frontend form.
/// </summary>
public sealed record PrivateBudgetAiResponseDto(
    Guid? CustomerId,
    string? Number,
    DateOnly? IssueDate,
    DateOnly? ValidUntil,
    string? Currency,
    string? Notes,
    string? TermsAndConditions,
    List<MatchedItemDto> Items
);
