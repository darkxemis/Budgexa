namespace Budgexa.Infrastructure.Services;

using System.Globalization;
using System.Text;
using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Common.Services;
using Budgexa.Domain.Constants;
using Microsoft.EntityFrameworkCore;

public sealed class ItemMatchingService(IApplicationDbContext db) : IItemMatchingService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "del", "la", "el", "los", "las", "un", "una", "y", "e", "o", "con", "para", "por",
        "the", "a", "an", "of", "and", "or", "for", "with", "in", "on",
        "der", "die", "das", "und", "mit", "von", "für", "ein", "eine",
        "i", "za", "na", "od", "sa", "u", "iz"
    };

    private const double MatchThreshold = 0.5;

    public async Task<List<MatchedItemDto>> MatchItemsAsync(
        Guid companyId,
        List<AiItemDto> aiItems,
        CancellationToken cancellationToken = default)
    {
        var companyItems = await db.Items
            .AsNoTracking()
            .Where(i => i.CompanyId == companyId && i.StatusId != StatusIds.Delete)
            .Select(i => new { i.Id, i.Name, i.UnitPrice, i.TaxRate, i.Unit, i.UnitMeasure })
            .ToListAsync(cancellationToken);

        var matchedItems = new List<MatchedItemDto>();

        foreach (var aiItem in aiItems)
        {
            var bestMatch = companyItems
                .Select(item => new { Item = item, Score = CalculateMatchScore(aiItem.ProductName, item.Name) })
                .Where(x => x.Score >= MatchThreshold)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Item.Name.Length) // Tie-breaker: shorter name = more specific
                .FirstOrDefault();

            if (bestMatch is not null)
            {
                matchedItems.Add(new MatchedItemDto(
                    ItemId: bestMatch.Item.Id,
                    ProductName: bestMatch.Item.Name,
                    Quantity: aiItem.Quantity,
                    DiscountPercentage: aiItem.DiscountPercentage,
                    UnitPrice: bestMatch.Item.UnitPrice,
                    TaxRate: bestMatch.Item.TaxRate,
                    Unit: bestMatch.Item.Unit,
                    UnitMeasure: (int?)bestMatch.Item.UnitMeasure
                ));
            }
            else
            {
                // No match found: return with null ItemId and placeholder data
                matchedItems.Add(new MatchedItemDto(
                    ItemId: null,
                    ProductName: aiItem.ProductName,
                    Quantity: aiItem.Quantity,
                    DiscountPercentage: aiItem.DiscountPercentage,
                    UnitPrice: null,
                    TaxRate: null,
                    Unit: null,
                    UnitMeasure: null
                ));
            }
        }

        return matchedItems;
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
