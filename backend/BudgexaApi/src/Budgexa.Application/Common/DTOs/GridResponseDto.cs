namespace Budgexa.Application.Common.DTOs;

public sealed record GridResponseDto<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
