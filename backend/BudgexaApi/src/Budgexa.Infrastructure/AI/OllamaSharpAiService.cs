namespace Budgexa.Infrastructure.AI;

using Budgexa.Application.PublicBudgets.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System.Text.RegularExpressions;

public sealed class OllamaSharpAiService(
    IConfiguration configuration,
    ILogger<OllamaSharpAiService> logger
) : IAiService
{
    private readonly string _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    private readonly string _defaultModel = configuration["Ollama:DefaultModel"] ?? "qwen2.5:7b";

    public async Task<AiJsonResult> GenerateJsonAsync(
        string systemPrompt,
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        var ollama = new OllamaApiClient(_baseUrl);

        var messages = new List<Message>
        {
            new(ChatRole.System, systemPrompt),
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

        logger.LogInformation("Calling Ollama at {BaseUrl} with model {Model}", _baseUrl, _defaultModel);

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
            return new AiJsonResult("[]", userRequest, _defaultModel);
        }

        logger.LogInformation("Ollama response: {Response}", fullResponse);

        if (string.IsNullOrWhiteSpace(fullResponse))
        {
            logger.LogWarning("Ollama returned empty response");
            return new AiJsonResult("[]", userRequest, _defaultModel);
        }

        var jsonContent = ExtractJson(fullResponse);
        jsonContent = FixDuplicateKeysJson(jsonContent);

        return new AiJsonResult(
            jsonContent,
            userRequest,
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

        // Remove markdown code blocks if present
        if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            var lines = trimmed.Split('\n');
            var jsonLines = lines.Skip(1).TakeWhile(line => !line.Trim().StartsWith("```")).ToArray();
            trimmed = string.Join('\n', jsonLines).Trim();
        }
        else if (trimmed.StartsWith("```"))
        {
            var lines = trimmed.Split('\n');
            var jsonLines = lines.Skip(1).TakeWhile(line => !line.Trim().StartsWith("```")).ToArray();
            trimmed = string.Join('\n', jsonLines).Trim();
        }

        // If it's already valid JSON (array or object), return as-is
        if ((trimmed.StartsWith('[') && trimmed.EndsWith(']')) ||
            (trimmed.StartsWith('{') && trimmed.EndsWith('}')))
        {
            return trimmed;
        }

        // Try to find an array in the response
        int arrayStart = response.IndexOf('[');
        int arrayEnd = response.LastIndexOf(']');

        if (arrayStart >= 0 && arrayEnd > arrayStart)
            return response.Substring(arrayStart, arrayEnd - arrayStart + 1);

        // Try to find a single object in the response
        int objStart = response.IndexOf('{');
        int objEnd = response.LastIndexOf('}');

        if (objStart >= 0 && objEnd > objStart)
            return response.Substring(objStart, objEnd - objStart + 1);

        return "[]";
    }
}

