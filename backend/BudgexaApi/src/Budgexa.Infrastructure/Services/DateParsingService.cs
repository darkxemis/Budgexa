namespace Budgexa.Infrastructure.Services;

using System.Globalization;
using Budgexa.Application.Common.Services;

public sealed class DateParsingService : IDateParsingService
{
    private static readonly CultureInfo[] SupportedCultures =
    [
        new CultureInfo("es-ES"), // Spanish
        new CultureInfo("en-US"), // US English
        new CultureInfo("en-GB"), // UK English
        new CultureInfo("de-DE"), // German
        CultureInfo.InvariantCulture // ISO
    ];

    public DateOnly? ParseDate(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        // Try ISO format first (fastest, most common in APIs)
        if (DateOnly.TryParse(dateString, CultureInfo.InvariantCulture, out var isoDate))
            return isoDate;

        // Try all supported cultures
        foreach (var culture in SupportedCultures)
        {
            if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out var dt))
                return DateOnly.FromDateTime(dt);
        }

        // Try ParseExact for common patterns
        if (TryParseCommonFormats(dateString, out var commonDate))
            return commonDate;

        return null;
    }

    private static bool TryParseCommonFormats(string dateString, out DateOnly result)
    {
        result = default;

        var formats = new[]
        {
            "d/M/yyyy", "dd/MM/yyyy", "M/d/yyyy", "MM/dd/yyyy",
            "d-M-yyyy", "dd-MM-yyyy", "M-d-yyyy", "MM-dd-yyyy",
            "d de MMMM de yyyy", "dd de MMMM de yyyy",
            "MMMM d, yyyy", "MMMM dd, yyyy",
            "yyyy-MM-dd", "yyyyMMdd"
        };

        foreach (var culture in SupportedCultures)
        {
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, culture, DateTimeStyles.None, out var dt))
                {
                    result = DateOnly.FromDateTime(dt);
                    return true;
                }
            }
        }

        return false;
    }
}
