namespace Budgexa.Infrastructure.Services;

using System.Globalization;
using System.Text;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Common.Services;
using Budgexa.Domain.Constants;
using Microsoft.EntityFrameworkCore;

public sealed class CustomerMatchingService(IApplicationDbContext db) : ICustomerMatchingService
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "del", "la", "el", "los", "las", "un", "una", "y", "e", "o", "con", "para", "por",
        "the", "a", "an", "of", "and", "or", "for", "with", "in", "on",
        "der", "die", "das", "und", "mit", "von", "für", "ein", "eine",
        "i", "za", "na", "od", "sa", "u", "iz", "s", "a", "y"
    };

    private const double MatchThreshold = 0.5;

    public async Task<Guid?> FindCustomerIdAsync(
        Guid companyId,
        string? customerName,
        string? customerTaxId,
        CancellationToken cancellationToken = default)
    {
        // Priority 1: Search by exact TaxId (normalized)
        if (!string.IsNullOrWhiteSpace(customerTaxId))
        {
            var normalizedTaxId = NormalizeTaxId(customerTaxId);

            var customerByTaxId = await db.Customers
                .AsNoTracking()
                .Where(c => c.CompanyId == companyId && c.StatusId != StatusIds.Delete)
                .ToListAsync(cancellationToken);

            var exactMatch = customerByTaxId
                .FirstOrDefault(c => NormalizeTaxId(c.TaxId).Equals(normalizedTaxId, StringComparison.OrdinalIgnoreCase));

            if (exactMatch is not null)
                return exactMatch.Id;
        }

        // Priority 2: Search by fuzzy name matching
        if (!string.IsNullOrWhiteSpace(customerName))
        {
            var customers = await db.Customers
                .AsNoTracking()
                .Where(c => c.CompanyId == companyId && c.StatusId != StatusIds.Delete)
                .Select(c => new { c.Id, c.LegalName, c.TradeName })
                .ToListAsync(cancellationToken);

            var bestMatch = customers
                .Select(c => new
                {
                    c.Id,
                    ScoreLegal = CalculateMatchScore(customerName, c.LegalName),
                    ScoreTrade = string.IsNullOrWhiteSpace(c.TradeName) ? 0 : CalculateMatchScore(customerName, c.TradeName)
                })
                .Select(c => new { c.Id, Score = Math.Max(c.ScoreLegal, c.ScoreTrade) })
                .Where(c => c.Score >= MatchThreshold)
                .OrderByDescending(c => c.Score)
                .ThenBy(c => c.Id) // Deterministic tie-breaker
                .FirstOrDefault();

            if (bestMatch is not null)
                return bestMatch.Id;
        }

        return null;
    }

    private static string NormalizeTaxId(string taxId)
    {
        // Remove spaces, hyphens, dots
        return taxId.Replace(" ", "").Replace("-", "").Replace(".", "");
    }

    private static double CalculateMatchScore(string aiName, string customerName)
    {
        var aiWords = GetSignificantWords(aiName);
        if (aiWords.Count == 0)
            return 0;

        var customerNormalized = Normalize(customerName);
        var matchedCount = aiWords.Count(word => customerNormalized.Contains(word, StringComparison.Ordinal));

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
