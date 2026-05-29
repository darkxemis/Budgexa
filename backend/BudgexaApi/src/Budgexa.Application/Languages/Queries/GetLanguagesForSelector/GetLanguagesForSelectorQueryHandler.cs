namespace Budgexa.Application.Languages.Queries.GetLanguagesForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetLanguagesForSelectorQueryHandler(
    ILanguageRepository languageRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetLanguagesForSelectorQuery, List<SelectorDto>>
{
    public async Task<List<SelectorDto>> Handle(GetLanguagesForSelectorQuery request, CancellationToken cancellationToken)
    {
        var languages = await languageRepository.GetAllAsync(cancellationToken);
        var userLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var selectorItems = languages
            .Select(l =>
            {
                var translation = l.Translations
                    .FirstOrDefault(t => t.TranslationLanguageId == userLanguageId);

                var displayName = translation?.Translation ?? l.Name;

                return new SelectorDto(l.Id, displayName);
            })
            .ToList();

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
