namespace Budgexa.Domain.Interfaces;

using Budgexa.Domain.Entities;

public interface ILanguageRepository
{
    Task<List<Language>> GetAllAsync(CancellationToken cancellationToken = default);
}