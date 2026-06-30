namespace Budgexa.Application.Languages.Queries.GetLanguagesForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetLanguagesForSelectorQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetLanguagesForSelectorQuery, List<SelectorDto>>
{
    public async Task<List<SelectorDto>> Handle(GetLanguagesForSelectorQuery request, CancellationToken cancellationToken)
    {
        var userLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var selectorItems = await db.Languages
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => new SelectorDto(
                l.Id,
                l.Translations
                    .Where(t => t.TranslationLanguageId == userLanguageId)
                    .Select(t => t.Translation)
                    .FirstOrDefault() ?? l.Name))
            .ToListAsync(cancellationToken);

        // Apply search filter if specified
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            selectorItems = selectorItems
                .Where(l => l.Name.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return selectorItems;
    }
}
