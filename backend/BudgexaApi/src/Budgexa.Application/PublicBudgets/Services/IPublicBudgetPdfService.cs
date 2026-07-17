namespace Budgexa.Application.PublicBudgets.Services;

using Budgexa.Domain.Entities;

public interface IPublicBudgetPdfService
{
    byte[] GeneratePdf(PublicBudget budget, Company company, string languageCode);
}
