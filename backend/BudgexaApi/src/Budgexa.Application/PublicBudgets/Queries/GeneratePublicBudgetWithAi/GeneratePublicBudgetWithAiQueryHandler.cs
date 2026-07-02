namespace Budgexa.Application.PublicBudgets.Queries.GeneratePublicBudgetWithAi;

using System.Globalization;
using System.Net;
using System.Text;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.PublicBudgets.DTOs;
using Budgexa.Application.PublicBudgets.Services;
using Budgexa.Domain.Constants;
using Budgexa.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GeneratePublicBudgetWithAiQueryHandler(
    IAiService aiService,
    IApplicationDbContext db)
    : IRequestHandler<GeneratePublicBudgetWithAiQuery, PublicBudgetAiResponseDto>
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "del", "la", "el", "los", "las", "un", "una", "y", "e", "o", "con", "para", "por",
        "the", "a", "an", "of", "and", "or", "for", "with", "in", "on",
        "der", "die", "das", "und", "mit", "von", "für", "ein", "eine",
        "i", "za", "na", "od", "sa", "u", "iz"
    };

    private const double MatchThreshold = 0.5;

    public async Task<PublicBudgetAiResponseDto> Handle(
        GeneratePublicBudgetWithAiQuery request,
        CancellationToken cancellationToken)
    {
        var companyId = request.Request.CompanyId;

        var companyExists = await db.Companies
            .AsNoTracking()
            .AnyAsync(c => c.Id == companyId, cancellationToken);

        if (!companyExists)
        {
            throw new AppException(
                HttpStatusCode.NotFound,
                ErrorTags.PublicBudget.CompanyNotFound,
                "Company not found.");
        }

        var aiResult = await aiService.GenerateBudgetJsonAsync(
            request.Request.UserRequest, cancellationToken);

        var companyItems = await db.Items
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId && i.StatusId != StatusIds.Delete)
            .Select(i => new { i.Id, i.Name, i.UnitPrice, i.TaxRate, i.Unit })
            .ToListAsync(cancellationToken);

        var matchedItems = new List<PublicBudgetAiItemDto>();

        foreach (var aiItem in aiResult.Items)
        {
            var bestMatch = companyItems
                .Select(item => new { Item = item, Score = CalculateMatchScore(aiItem.ProductName, item.Name) }) // Calculate match score for each item against the AI-extracted name
                .Where(x => x.Score >= MatchThreshold) // Filter out items below 50% word match
                .OrderByDescending(x => x.Score) // Best score first (highest similarity wins)
                .ThenBy(x => x.Item.Name.Length) // Tie-breaker: shorter name = more specific match
                .FirstOrDefault(); // Pick the single best match

            if (bestMatch is not null)
            {
                matchedItems.Add(new PublicBudgetAiItemDto(
                    bestMatch.Item.Id,
                    bestMatch.Item.Name,
                    bestMatch.Item.UnitPrice,
                    bestMatch.Item.TaxRate,
                    bestMatch.Item.Unit,
                    aiItem.Quantity));
            }
        }

        return new PublicBudgetAiResponseDto(
            aiResult.OriginalRequest,
            matchedItems,
            aiResult.Model);
    }

    private static double CalculateMatchScore(string aiName, string itemName)
    {
        var aiWords = GetSignificantWords(aiName);
        if (aiWords.Count == 0)
            return 0;

        var itemNormalized = Normalize(itemName);
        var matchedCount = aiWords.Count(word => itemNormalized.Contains(word, StringComparison.Ordinal));

        return (double)matchedCount / aiWords.Count;
    }

    private static List<string> GetSignificantWords(string text)
    {
        return Normalize(text)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 3 && !StopWords.Contains(w))
            .ToList();
    }

    private static string Normalize(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}
