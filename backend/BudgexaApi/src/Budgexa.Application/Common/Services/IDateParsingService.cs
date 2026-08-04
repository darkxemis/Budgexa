namespace Budgexa.Application.Common.Services;

public interface IDateParsingService
{
    /// <summary>
    /// Parses a natural language date string into DateOnly.
    /// Supports multiple formats and cultures.
    /// </summary>
    /// <param name="dateString">Date string from AI (e.g., "15 de enero de 2025", "January 15, 2025")</param>
    /// <returns>DateOnly if parsing succeeds, null otherwise</returns>
    DateOnly? ParseDate(string? dateString);
}
