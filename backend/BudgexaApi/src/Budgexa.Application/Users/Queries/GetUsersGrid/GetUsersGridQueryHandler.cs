namespace Budgexa.Application.Users.Queries.GetUsersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Helpers;
using Budgexa.Application.Common.Interfaces;
using Budgexa.Application.Users.DTOs;
using Budgexa.Domain.Entities;
using Budgexa.Domain.Interfaces;
using MediatR;

public sealed class GetUsersGridQueryHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUserService
) : IRequestHandler<GetUsersGridQuery, GridResponseDto<UserGridDto>>
{
    public async Task<GridResponseDto<UserGridDto>> Handle(GetUsersGridQuery request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var currentUserLanguageId = await currentUserService.GetLanguageIdAsync(cancellationToken);

        var gridifyFilters = GridifyHelper.BuildFilterExpression(dto.Filters);
        var gridifySorting = GridifyHelper.BuildSortingExpression(dto.Sorting);

        var (users, totalCount) = await userRepository.GetGridAsync(
            currentUserService.CompanyId,
            currentUserLanguageId,
            dto.Page,
            dto.PageSize,
            gridifySorting,
            gridifyFilters,
            dto.Search,
            cancellationToken);

        var userGridDtos = users.Select(user =>
        {
            var languageName = GetLanguageTranslation(
                user.Language.Translations,
                currentUserLanguageId,
                user.Language.Name);

            var statusName = GetStatusTranslation(
                user.Status.Translations,
                currentUserLanguageId,
                user.Status.Name);

            return new UserGridDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.CompanyId,
                user.Company.Name,
                user.LanguageId,
                languageName,
                user.StatusId,
                statusName,
                user.CreatedAt,
                user.UpdatedAt,
                user.UserRoles.Select(ur => new RoleInfo(ur.Role.Id, ur.Role.Name)).ToList());
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / dto.PageSize);

        return new GridResponseDto<UserGridDto>(
            userGridDtos,
            totalCount,
            dto.Page,
            dto.PageSize,
            totalPages);
    }

    private static string GetLanguageTranslation(
        ICollection<LanguageTranslation> translations,
        Guid languageId,
        string defaultName)
    {
        var translation = translations.FirstOrDefault(t => t.TranslationLanguageId == languageId);
        return translation?.Translation ?? defaultName;
    }

    private static string GetStatusTranslation(
        ICollection<StatusTranslation> translations,
        Guid languageId,
        string defaultName)
    {
        var translation = translations.FirstOrDefault(t => t.LanguageId == languageId);
        return translation?.Translation ?? defaultName;
    }
}
