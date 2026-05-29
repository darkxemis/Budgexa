namespace Budgexa.Application.Status.Queries.GetStatusForSelector;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetStatusForSelectorQueryHandler(
    IStatusRepository statusRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetStatusForSelectorQuery, List<SelectorDto>>
{
    public async Task<List<SelectorDto>> Handle(GetStatusForSelectorQuery request, CancellationToken cancellationToken)
    {
        var statuses = await statusRepository.GetAllAsync(cancellationToken);
        var userLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var query = statuses.AsEnumerable();

        // Filter by group if specified
        if (!string.IsNullOrWhiteSpace(request.Group))
        {
            query = query.Where(s => s.Group.Equals(request.Group, StringComparison.OrdinalIgnoreCase));
        }

        var selectorItems = query
            .Select(s =>
            {
                var translation = s.Translations
                    .FirstOrDefault(t => t.LanguageId == userLanguageId);

                var displayName = translation?.Translation ?? s.Name;

                return new SelectorDto(s.Id, displayName);
            })
            .ToList();

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
