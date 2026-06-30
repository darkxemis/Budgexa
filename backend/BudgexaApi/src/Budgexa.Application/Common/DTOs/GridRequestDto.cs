namespace Budgexa.Application.Common.DTOs;

public sealed record GridRequestDto(
    int Page,
    int PageSize,
    List<GridSortDto>? Sorting,
    List<GridFilterDto>? Filters,
    string? Search);

public sealed record GridFilterDto(
    string Column,
    string Operator,
    string? Value);

public sealed record GridSortDto(
    string Column,
    bool IsDescending);

/// <summary>
/// Gridify supported operators:
/// - Equal: =
/// - NotEqual: !=
/// - LessThan: <
/// - GreaterThan: >
/// - GreaterThanOrEqual: >=
/// - LessThanOrEqual: <=
/// - Contains (Like): =*
/// - NotContains (NotLike): !*
/// - StartsWith: ^
/// - NotStartsWith: !^
/// - EndsWith: $
/// - NotEndsWith: !$
/// </summary>
public static class GridifyOperators
{
    public const string Equal = "=";
    public const string NotEqual = "!=";
    public const string LessThan = "<";
    public const string GreaterThan = ">";
    public const string GreaterThanOrEqual = ">=";
    public const string LessThanOrEqual = "<=";
    public const string Contains = "=*";
    public const string NotContains = "!*";
    public const string StartsWith = "^";
    public const string NotStartsWith = "!^";
    public const string EndsWith = "$";
    public const string NotEndsWith = "!$";
}
