namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// Raw AI extraction for private invoice (before matching/parsing).
/// All fields nullable - maps to PrivateInvoicePrompt JSON.
/// </summary>
public sealed record AiPrivateInvoiceDto(
    string? CustomerName,
    string? CustomerTaxId,
    string? Series,
    string? Number,
    string? IssueDate,
    string? DueDate,
    string? Currency,
    string? Notes,
    List<AiItemDto>? Items
);
