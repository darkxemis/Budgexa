namespace Budgexa.Application.Languages.Queries.GetAllLanguages;

using Budgexa.Application.Languages.DTOs;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetAllLanguagesQueryHandler(
    ILanguageRepository languageRepository
) : IRequestHandler<GetAllLanguagesQuery, List<LanguageDto>>
{
    public async Task<List<LanguageDto>> Handle(GetAllLanguagesQuery request, CancellationToken cancellationToken)
    {
        var languages = await languageRepository.GetAllAsync(cancellationToken);

        return languages
            .Select(l => new LanguageDto(l.Id, l.Code, l.Name))
            .ToList();
    }
}