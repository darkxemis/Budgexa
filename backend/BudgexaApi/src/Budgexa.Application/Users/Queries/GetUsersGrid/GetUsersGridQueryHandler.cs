namespace Budgexa.Application.Users.Queries.GetUsersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Helpers;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Enums;
using Gridify;
using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class GetUsersGridQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUserService
) : IRequestHandler<GetUsersGridQuery, GridResponseDto<UserGridDto>>
{
    public async Task<GridResponseDto<UserGridDto>> Handle(GetUsersGridQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var companyId = currentUserService.CompanyId;
        var currentUserLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var gridifyFilters = GridifyHelper.BuildFilterExpression(dto.Filters);
        var gridifySorting = GridifyHelper.BuildSortingExpression(dto.Sorting);

        IQueryable<User> query = db.Users
            .AsNoTracking()
            .Where(u => u.CompanyId == companyId)
            .Where(u => u.Status.Value != (int)BaseStatus.Delete);

        if (!string.IsNullOrWhiteSpace(dto.Search))
        {
            var searchLower = dto.Search.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower) ||
                u.Company.Name.ToLower().Contains(searchLower) ||
                u.Language.Name.ToLower().Contains(searchLower));
        }

        var mapper = GetUserGridMapper(currentUserLanguageId);

        if (!string.IsNullOrWhiteSpace(gridifyFilters))
        {
            query = query.ApplyFiltering(gridifyFilters, mapper);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(gridifySorting))
        {
            query = query.ApplyOrdering(gridifySorting, mapper);
        }
        else
        {
            query = query.OrderBy(u => u.CreatedAt);
        }

        var projected = query.Select(u => new UserGridDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.CompanyId,
            u.Company.Name,
            u.LanguageId,
            u.Language.Translations
                .Where(t => t.TranslationLanguageId == currentUserLanguageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? u.Language.Name,
            u.StatusId,
            u.Status.Translations
                .Where(t => t.LanguageId == currentUserLanguageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? u.Status.Name,
            u.CreatedAt,
            u.UpdatedAt,
            u.UserRoles.Select(ur => new RoleInfo(ur.Role.Id, ur.Role.Name)).ToList()));

        return await projected.ToGridResponseAsync(dto.Page, dto.PageSize, totalCount, cancellationToken);
    }

    private static IGridifyMapper<User> GetUserGridMapper(Guid currentUserLanguageId)
    {
        var mapper = new GridifyMapper<User>()
            .GenerateMappings()
            .AddMap("Id", u => u.Id)
            .AddMap("Email", u => u.Email)
            .AddMap("FirstName", u => u.FirstName)
            .AddMap("LastName", u => u.LastName)
            .AddMap("CompanyId", u => u.CompanyId)
            .AddMap("CompanyName", u => u.Company.Name)
            .AddMap("LanguageId", u => u.LanguageId)
            .AddMap("LanguageName", u => u.Language.Translations
                .Where(t => t.TranslationLanguageId == currentUserLanguageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? u.Language.Name)
            .AddMap("StatusId", u => u.StatusId)
            .AddMap("StatusName", u => u.Status.Translations
                .Where(t => t.LanguageId == currentUserLanguageId)
                .Select(t => t.Translation)
                .FirstOrDefault() ?? u.Status.Name)
            .AddMap("CreatedAt", u => u.CreatedAt.Date)
            .AddMap("UpdatedAt", u => u.UpdatedAt.HasValue ? u.UpdatedAt.Value.Date : null);

        return mapper;
    }
}
