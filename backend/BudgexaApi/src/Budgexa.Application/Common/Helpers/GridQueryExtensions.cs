namespace Budgexa.Application.Common.Helpers;

using Budgexa.Application.Common.DTOs;
using Microsoft.EntityFrameworkCore;

public static class GridQueryExtensions
{
    /// <summary>
    /// Applies pagination to a projected query and materializes it into a <see cref="GridResponseDto{TDto}"/>.
    /// The caller is responsible for computing <paramref name="totalCount"/> against the filtered query
    /// before projection/ordering.
    /// </summary>
    public static async Task<GridResponseDto<TDto>> ToGridResponseAsync<TDto>(
        this IQueryable<TDto> projected,
        int page,
        int pageSize,
        int totalCount,
        CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;

        var items = await projected
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var totalPages = pageSize > 0
            ? (int)Math.Ceiling((double)totalCount / pageSize)
            : 0;

        return new GridResponseDto<TDto>(items, totalCount, page, pageSize, totalPages);
    }
}
