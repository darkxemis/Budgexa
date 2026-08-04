namespace Budgexa.Application.PublicBudgets.Services;

using Budgexa.Application.PublicBudgets.DTOs;

public interface IAiService
{
    /// <summary>
    /// Generates JSON from user request using specified system prompt.
    /// Returns raw JSON string for the caller to deserialize as needed.
    /// </summary>
    Task<AiJsonResult> GenerateJsonAsync(string systemPrompt, string userRequest, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic AI JSON generation result containing raw JSON string.
/// </summary>
public sealed record AiJsonResult(
    string JsonResponse,
    string OriginalRequest,
    string Model
);
