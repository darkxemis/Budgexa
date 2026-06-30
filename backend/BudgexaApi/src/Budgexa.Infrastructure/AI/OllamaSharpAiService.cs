namespace Budgexa.Infrastructure.AI;

using Budgexa.Application.Budgets.DTOs;
using Budgexa.Application.Budgets.Services;
using Microsoft.Extensions.Configuration;
using OllamaSharp;
using OllamaSharp.Models;
using System.Text.Json;

public sealed class OllamaSharpAiService(
    IConfiguration configuration
) : IAiService
{
    private readonly string _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
    private readonly string _defaultModel = configuration["Ollama:DefaultModel"] ?? "llama3.2";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<BudgetItemsAiResult> GenerateBudgetJsonAsync(
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(userRequest);

        var ollama = new OllamaApiClient(_baseUrl);

        var request = new GenerateRequest
        {
            Model = _defaultModel,
            Prompt = prompt,
            Stream = false,
            Format = "json",
            Options = new RequestOptions
            {
                Temperature = 0
            }
        };

        var fullResponse = string.Empty;
        
        await foreach (var stream in ollama.GenerateAsync(request, cancellationToken))
        {
            if (stream is { Done: true })
            {
                fullResponse = stream.Response;
            }
        }

        if (string.IsNullOrWhiteSpace(fullResponse))
            return new BudgetItemsAiResult(userRequest, [], _defaultModel);

        var jsonContent = ExtractJson(fullResponse);

        var items = JsonSerializer.Deserialize<List<BudgetItem>>(jsonContent, JsonOptions) ?? [];

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

    private static string ExtractJson(string response)
    {
        // Si ya es JSON válido, devolverlo
        if (response.TrimStart().StartsWith('['))
            return response;

        // Buscar JSON en la respuesta
        int start = response.IndexOf('[');
        int end = response.LastIndexOf(']');

        if (start >= 0 && end > start)
            return response.Substring(start, end - start + 1);

        return response;
    }
}

