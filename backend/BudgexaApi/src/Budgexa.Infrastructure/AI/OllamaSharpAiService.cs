namespace Budgexa.Infrastructure.AI;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.PublicBudgets.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text.Json;
using System.Text.RegularExpressions;

public sealed class OllamaSharpAiService(
    IConfiguration configuration,
    ILogger<OllamaSharpAiService> logger
) : IAiService
{
    private readonly string _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    private readonly string _defaultModel = configuration["Ollama:DefaultModel"] ?? "qwen2.5:7b";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemMessage = """
        You extract products/services from text into JSON.
        Return ONLY a JSON array, nothing else.
        Each item: {"productName": "Full Name", "quantity": N}
        CRITICAL RULES:
        - productName must contain ONLY the product name, NEVER include quantities or numbers.
          Example: "4 ventanas de aluminio" -> {"productName": "Ventana De Aluminio", "quantity": 4}
          Example: "3 puertas de madera" -> {"productName": "Puerta De Madera", "quantity": 3}
        - Keep product names in the SAME language the user wrote. NEVER translate or mix languages.
        - Use the FULL product name (e.g. "Ventana De Aluminio Reforzado" not just "Aluminio").
        - If quantity is not stated, use 1.
        """;

    public async Task<BudgetItemsAiResult> GenerateBudgetJsonAsync(
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        var ollama = new OllamaApiClient(_baseUrl);

        var messages = new List<Message>
        {
            new(ChatRole.System, SystemMessage),
            new(ChatRole.User, userRequest)
        };

        var request = new ChatRequest
        {
            Model = _defaultModel,
            Messages = messages,
            Stream = false,
            Options = new OllamaSharp.Models.RequestOptions
            {
                Temperature = 0,
                NumPredict = 2048
            }
        };

        var fullResponse = string.Empty;

        logger.LogInformation("Calling Ollama at {BaseUrl} with model {Model}. User request: {UserRequest}", _baseUrl, _defaultModel, userRequest);

        try
        {
            await foreach (var chunk in ollama.ChatAsync(request, cancellationToken))
            {
                if (chunk?.Done == true)
                {
                    fullResponse = chunk.Message?.Content ?? string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call Ollama at {BaseUrl}", _baseUrl);
            return new BudgetItemsAiResult(userRequest, [], _defaultModel);
        }

        logger.LogInformation("Ollama response: {Response}", fullResponse);

        if (string.IsNullOrWhiteSpace(fullResponse))
        {
            logger.LogWarning("Ollama returned empty response");
            return new BudgetItemsAiResult(userRequest, [], _defaultModel);
        }

        var jsonContent = ExtractJson(fullResponse);
        jsonContent = FixDuplicateKeysJson(jsonContent);

        var items = JsonSerializer.Deserialize<List<BudgetItem>>(jsonContent, JsonOptions) ?? [];

        return new BudgetItemsAiResult(
            userRequest,
            items,
            _defaultModel);
    }

    private static string FixDuplicateKeysJson(string json)
    {
        var productNameCount = Regex.Matches(json, @"""productName""", RegexOptions.IgnoreCase).Count;

        if (productNameCount <= 1)
            return json;

        var objectCount = Regex.Matches(json, @"\{").Count;
        if (objectCount >= productNameCount)
            return json;

        var pairs = Regex.Matches(json,
            @"""productName""\s*:\s*""([^""]+)""\s*(?:,\s*""quantity""\s*:\s*(\d+))?",
            RegexOptions.IgnoreCase);

        if (pairs.Count == 0)
            return json;

        var items = new List<string>();
        foreach (Match match in pairs)
        {
            var name = match.Groups[1].Value;
            var qty = match.Groups[2].Success ? match.Groups[2].Value : "1";
            items.Add($@"{{""productName"":""{name}"",""quantity"":{qty}}}");
        }

        return $"[{string.Join(",", items)}]";
    }

    private static string ExtractJson(string response)
    {
        var trimmed = response.Trim();

        // Already a valid JSON array
        if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            return trimmed;

        // Single JSON object — wrap in array
        if (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
            return $"[{trimmed}]";

        // Try to find an array in the response (may have markdown or extra text)
        int arrayStart = response.IndexOf('[');
        int arrayEnd = response.LastIndexOf(']');

        if (arrayStart >= 0 && arrayEnd > arrayStart)
            return response.Substring(arrayStart, arrayEnd - arrayStart + 1);

        // Try to find a single object in the response
        int objStart = response.IndexOf('{');
        int objEnd = response.LastIndexOf('}');

        if (objStart >= 0 && objEnd > objStart)
            return $"[{response.Substring(objStart, objEnd - objStart + 1)}]";

        return "[]";
    }
}

