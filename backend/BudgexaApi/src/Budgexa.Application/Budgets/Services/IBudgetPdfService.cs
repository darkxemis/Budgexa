namespace Budgexa.Application.Budgets.Services;

using Budgexa.Domain.Entities;

public interface IBudgetPdfService
{
    byte[] GeneratePdf(Budget budget, Company company, Customer customer, byte[]? signatureBytes, string languageCode);
}
