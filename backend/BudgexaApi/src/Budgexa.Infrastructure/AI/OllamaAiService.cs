namespace Budgexa.Infrastructure.AI;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Services;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class OllamaAiService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : IAiService
{
    private readonly string _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    private readonly string _defaultModel = configuration["Ollama:DefaultModel"] ?? "llama3.2";

    public async Task<BudgetItemsAiResult> GenerateBudgetJsonAsync(
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(userRequest);
        var ollamaRequest = new
        {
            model = _defaultModel,
            prompt = prompt,
            stream = false,
            options = new { temperature = 0 }
        };

        var json = JsonSerializer.Serialize(ollamaRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMinutes(2);

        var response = await client.PostAsync(
            $"{_baseUrl}/api/generate",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseBody);

        var items = ParseBudgetItems(ollamaResponse?.Response);

        return new BudgetItemsAiResult(
            userRequest,
            items,
            _defaultModel);
    }

    private static string BuildPrompt(string userRequest)
    {
        return $@"
            Extract products and quantities from the following text.

            Text:
            ""{userRequest}""

            Return ONLY valid JSON.
            Exact format:
            [
              {{
                ""productName"": ""string"",
                ""quantity"": number
              }}
            ]
            ";
    }

    private static List<BudgetItem> ParseBudgetItems(string? rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return [];
        }

        int start = rawResponse.IndexOf('[');
        int end = rawResponse.LastIndexOf(']');

        if (start == -1 || end == -1 || end <= start)
        {
            return [];
        }

        string jsonArray = rawResponse.Substring(start, end - start + 1);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<BudgetItem>>(jsonArray, options) ?? [];
    }

    private sealed record OllamaResponse(
        [property: JsonPropertyName("response")] string Response,
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("done")] bool Done
    );
}
