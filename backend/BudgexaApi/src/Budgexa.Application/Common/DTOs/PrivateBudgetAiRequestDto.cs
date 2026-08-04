namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// Request DTO for authenticated budget generation with AI.
/// </summary>
public sealed record PrivateBudgetAiRequestDto(
    string UserRequest
);
