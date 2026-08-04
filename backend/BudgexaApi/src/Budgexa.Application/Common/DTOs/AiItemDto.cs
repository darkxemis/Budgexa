namespace Budgexa.Application.Common.DTOs;

/// <summary>
/// AI-extracted item (raw from Ollama response).
/// </summary>
public sealed record AiItemDto(
    string ProductName,
    decimal Quantity,
    decimal? DiscountPercentage = null
);
