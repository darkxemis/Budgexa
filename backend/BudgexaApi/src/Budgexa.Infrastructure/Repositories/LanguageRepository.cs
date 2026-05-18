namespace Budgexa.Infrastructure.Repositories;

using Budgexa.Domain.Entities;
using Budgexa.Domain.Interfaces;
using Budgexa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class LanguageRepository(
    ApplicationDbContext dbContext
) : ILanguageRepository
{
    public async Task<List<Language>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Languages
            .Include(l => l.Translations)
                .ThenInclude(t => t.TranslationLanguage)
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }
}