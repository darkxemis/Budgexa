namespace Budgexa.Application.Languages.Queries.GetAllLanguages;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Languages.DTOs;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetAllLanguagesQueryHandler(
    ILanguageRepository languageRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllLanguagesQuery, List<LanguageDto>>
{
    public async Task<List<LanguageDto>> Handle(GetAllLanguagesQuery request, CancellationToken cancellationToken)
    {
        var languages = await languageRepository.GetAllAsync(cancellationToken);
        var userLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return languages
            .Select(l =>
            {
                var translation = l.Translations
                    .FirstOrDefault(t => t.TranslationLanguageId == userLanguageId);

                var displayName = translation?.Translation ?? l.Name;

                return new LanguageDto(l.Id, l.Code, displayName);
            })
            .ToList();
    }
}