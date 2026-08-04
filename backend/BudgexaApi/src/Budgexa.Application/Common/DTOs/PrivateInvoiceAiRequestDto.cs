namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// Request DTO for authenticated invoice generation with AI.
/// </summary>
public sealed record PrivateInvoiceAiRequestDto(
    string UserRequest
);
