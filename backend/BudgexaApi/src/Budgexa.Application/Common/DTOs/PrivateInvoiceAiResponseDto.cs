namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// Response DTO for authenticated invoice generation with AI.
/// Contains only essential fields needed by the frontend form.
/// </summary>
public sealed record PrivateInvoiceAiResponseDto(
    Guid? CustomerId,
    string? Series,
    string? Number,
    DateOnly? IssueDate,
    DateOnly? DueDate,
    string? Currency,
    string? Notes,
    List<MatchedItemDto> Items
);
