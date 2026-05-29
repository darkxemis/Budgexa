namespace Budgexa.Application.Common.Validators;

using Budgexa.Application.Common.DTOs;
using FluentValidation;

/// <summary>
/// Validator for grid request to ensure data integrity and prevent malicious input.
/// </summary>
public sealed class GridRequestDtoValidator : AbstractValidator<GridRequestDto>
{
    private const int MaxPageSize = 100;
    private const int MaxFilters = 50;
    private const int MaxSorting = 10;
    private const int MaxSearchLength = 500;
    private const int MaxColumnNameLength = 100;

    public GridRequestDtoValidator()
    {
        // Page validation
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        // PageSize validation
        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(MaxPageSize).WithMessage($"PageSize cannot exceed {MaxPageSize}.");

        // Search validation
        RuleFor(x => x.Search)
            .MaximumLength(MaxSearchLength).WithMessage($"Search cannot exceed {MaxSearchLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        // Filters validation
        RuleFor(x => x.Filters)
            .Must(filters => filters == null || filters.Count <= MaxFilters)
            .WithMessage($"Cannot apply more than {MaxFilters} filters.");

        RuleForEach(x => x.Filters)
            .ChildRules(filter =>
            {
                filter.RuleFor(f => f.Column)
                    .NotEmpty().WithMessage("Filter column cannot be empty.")
                    .MaximumLength(MaxColumnNameLength).WithMessage($"Filter column name cannot exceed {MaxColumnNameLength} characters.")
                    .Matches("^[a-zA-Z][a-zA-Z0-9]*$").WithMessage("Filter column must be a valid property name (alphanumeric, starting with letter).");

                filter.RuleFor(f => f.Operator)
                    .NotEmpty().WithMessage("Filter operator cannot be empty.")
                    .Must(BeValidOperator).WithMessage("Invalid filter operator.");
            })
            .When(x => x.Filters != null);

        // Sorting validation
        RuleFor(x => x.Sorting)
            .Must(sorting => sorting == null || sorting.Count <= MaxSorting)
            .WithMessage($"Cannot sort by more than {MaxSorting} columns.");

        RuleForEach(x => x.Sorting)
            .ChildRules(sort =>
            {
                sort.RuleFor(s => s.Column)
                    .NotEmpty().WithMessage("Sort column cannot be empty.")
                    .MaximumLength(MaxColumnNameLength).WithMessage($"Sort column name cannot exceed {MaxColumnNameLength} characters.")
                    .Matches("^[a-zA-Z][a-zA-Z0-9]*$").WithMessage("Sort column must be a valid property name (alphanumeric, starting with letter).");
            })
            .When(x => x.Sorting != null);
    }

    private static bool BeValidOperator(string op)
    {
        if (string.IsNullOrWhiteSpace(op))
            return false;

        var validOperators = new[]
        {
            // Gridify operators
            GridifyOperators.Equal,
            GridifyOperators.NotEqual,
            GridifyOperators.LessThan,
            GridifyOperators.GreaterThan,
            GridifyOperators.GreaterThanOrEqual,
            GridifyOperators.LessThanOrEqual,
            GridifyOperators.Contains,
            GridifyOperators.NotContains,
            GridifyOperators.StartsWith,
            GridifyOperators.NotStartsWith,
            GridifyOperators.EndsWith,
            GridifyOperators.NotEndsWith,
            // Backwards compatibility
            "equals", "notequals", "lessthan", "greaterthan",
            "greaterthanorequal", "lessthanorequal",
            "contains", "notcontains", "startswith", "notstartswith",
            "endswith", "notendswith"
        };

        return validOperators.Contains(op, StringComparer.OrdinalIgnoreCase) || validOperators.Contains(op);
    }
}
