namespace Budgexa.Application.Common.Services;

using Budgexa.Application.Common.DTOs;

public interface IItemMatchingService
{
    /// <summary>
    /// Matches AI-extracted items against company items using fuzzy matching.
    /// Returns matched data with resolved IDs and unit prices.
    /// </summary>
    /// <param name="companyId">Company to search within</param>
    /// <param name="aiItems">AI-extracted items with productName, quantity, discountPercentage</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of matched items with resolved ItemId, UnitPrice, TaxRate, Unit, etc.</returns>
    Task<List<MatchedItemDto>> MatchItemsAsync(
        Guid companyId,
        List<AiItemDto> aiItems,
        CancellationToken cancellationToken = default);
}
