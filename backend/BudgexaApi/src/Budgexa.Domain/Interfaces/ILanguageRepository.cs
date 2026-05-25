namespace Budgexa.Domain.Interfaces;

using Budgexa.Domain.Entities;

public interface ILanguageRepository
{
    Task<List<Language>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Language?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}