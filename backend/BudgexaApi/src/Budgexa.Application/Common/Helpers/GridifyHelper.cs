namespace Budgexa.Application.Common.Helpers;

using Budgexa.Application.Common.DTOs;

public static class GridifyHelper
{
    public static string? BuildFilterExpression(List<GridFilterDto>? filters)
    {
        if (filters == null || filters.Count == 0)
            return null;

        var filterStrings = new List<string>();

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Column))
                continue;

            var filterExpression = filter.Operator switch
            {
                // Exact operators from Gridify
                GridifyOperators.Equal => $"{filter.Column}={filter.Value}",
                GridifyOperators.NotEqual => $"{filter.Column}!={filter.Value}",
                GridifyOperators.GreaterThan => $"{filter.Column}>{filter.Value}",
                GridifyOperators.GreaterThanOrEqual => $"{filter.Column}>={filter.Value}",
                GridifyOperators.LessThan => $"{filter.Column}<{filter.Value}",
                GridifyOperators.LessThanOrEqual => $"{filter.Column}<={filter.Value}",
                GridifyOperators.Contains => $"{filter.Column}=*{filter.Value}",
                GridifyOperators.NotContains => $"{filter.Column}!*{filter.Value}",
                GridifyOperators.StartsWith => $"{filter.Column}^{filter.Value}",
                GridifyOperators.NotStartsWith => $"{filter.Column}!^{filter.Value}",
                GridifyOperators.EndsWith => $"{filter.Column}${filter.Value}",
                GridifyOperators.NotEndsWith => $"{filter.Column}!${filter.Value}",
                // Backwards compatibility with string names
                "=" or "equals" => $"{filter.Column}={filter.Value}",
                "!=" or "notequals" => $"{filter.Column}!={filter.Value}",
                ">" or "greaterthan" => $"{filter.Column}>{filter.Value}",
                ">=" or "greaterthanorequal" => $"{filter.Column}>={filter.Value}",
                "<" or "lessthan" => $"{filter.Column}<{filter.Value}",
                "<=" or "lessthanorequal" => $"{filter.Column}<={filter.Value}",
                "contains" or "=*" => $"{filter.Column}=*{filter.Value}",
                "notcontains" or "!*" => $"{filter.Column}!*{filter.Value}",
                "startswith" or "^" => $"{filter.Column}^{filter.Value}",
                "notstartswith" or "!^" => $"{filter.Column}!^{filter.Value}",
                "endswith" or "$" => $"{filter.Column}${filter.Value}",
                "notendswith" or "!$" => $"{filter.Column}!${filter.Value}",
                _ => $"{filter.Column}={filter.Value}"
            };

            filterStrings.Add(filterExpression);
        }

        return string.Join(",", filterStrings);
    }

    public static string? BuildSortingExpression(List<GridSortDto>? sorting)
    {
        if (sorting == null || sorting.Count == 0)
            return null;

        var sortStrings = new List<string>();

        foreach (var sort in sorting)
        {
            if (string.IsNullOrWhiteSpace(sort.Column))
                continue;

            var direction = sort.IsDescending ? "desc" : "asc";
            sortStrings.Add($"{sort.Column} {direction}");
        }

        return string.Join(", ", sortStrings);
    }
}
