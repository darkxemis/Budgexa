namespace Budgexa.Application.Languages.Queries.GetAllLanguages;

using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Languages.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetAllLanguagesQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetAllLanguagesQuery, List<LanguageDto>>
{
    public async Task<List<LanguageDto>> Handle(GetAllLanguagesQuery request, CancellationToken cancellationToken)
    {
        var userLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        return await db.Languages
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => new LanguageDto(
                l.Id,
                l.Code,
                l.Translations
                    .Where(t => t.TranslationLanguageId == userLanguageId)
                    .Select(t => t.Translation)
                    .FirstOrDefault() ?? l.Name))
            .ToListAsync(cancellationToken);
    }
}