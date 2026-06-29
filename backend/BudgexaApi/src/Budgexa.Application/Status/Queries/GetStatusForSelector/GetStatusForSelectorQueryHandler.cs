namespace Budgexa.Application.Status.Queries.GetStatusForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetStatusForSelectorQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetStatusForSelectorQuery, List<SelectorDto>>
{
    public async Task<List<SelectorDto>> Handle(GetStatusForSelectorQuery request, CancellationToken cancellationToken)
    {
        var userLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var query = db.Statuses.AsNoTracking();

        // Filter by group if specified
        if (!string.IsNullOrWhiteSpace(request.Group))
        {
            var group = request.Group;
            query = query.Where(s => s.Group.ToLower() == group.ToLower());
        }

        var selectorItems = await query
            .Select(s => new SelectorDto(
                s.Id,
                s.Translations
                    .Where(t => t.LanguageId == userLanguageId)
                    .Select(t => t.Translation)
                    .FirstOrDefault() ?? s.Name))
            .ToListAsync(cancellationToken);

        // Apply search filter if specified
        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            selectorItems = selectorItems
                .Where(s => s.Name.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return selectorItems;
    }
}
