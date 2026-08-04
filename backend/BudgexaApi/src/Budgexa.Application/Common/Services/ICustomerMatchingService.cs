namespace Budgexa.Application.Common.Services;

public interface ICustomerMatchingService
{
    /// <summary>
    /// Finds a customer by tax ID or name using fuzzy matching.
    /// Priority: Exact TaxId match -> Fuzzy name match -> null.
    /// </summary>
    /// <param name="companyId">Company to search within</param>
    /// <param name="customerName">Customer name (nullable)</param>
    /// <param name="customerTaxId">Customer tax ID/NIF/CIF (nullable)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer Guid if found, null otherwise</returns>
    Task<Guid?> FindCustomerIdAsync(
        Guid companyId,
        string? customerName,
        string? customerTaxId,
        CancellationToken cancellationToken = default);
}
