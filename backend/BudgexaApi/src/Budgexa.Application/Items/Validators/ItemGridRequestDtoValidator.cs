namespace Budgexa.Application.Items.Validators;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Validators;
using FluentValidation;

public sealed class ItemGridRequestDtoValidator : AbstractValidator<GridRequestDto>
{
    private static readonly HashSet<string> GuidColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "StatusId"
    };

    private static readonly HashSet<string> DateTimeColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreatedAt", "UpdatedAt"
    };

    private static readonly HashSet<string> NumericColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "UnitPrice", "TaxRate", "Type"
    };

    private static readonly HashSet<string> ValidGuidOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        GridifyOperators.Equal,
        GridifyOperators.NotEqual
    };

    private static readonly HashSet<string> ValidComparisonOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        GridifyOperators.Equal,
        GridifyOperators.NotEqual,
        GridifyOperators.LessThan,
        GridifyOperators.GreaterThan,
        GridifyOperators.GreaterThanOrEqual,
        GridifyOperators.LessThanOrEqual
    };

    public ItemGridRequestDtoValidator()
    {
        Include(new GridRequestDtoValidator());

        RuleForEach(x => x.Filters)
            .Must(filter => BeValidOperatorForColumn(filter.Column, filter.Operator))
            .WithMessage((request, filter) => GetOperatorErrorMessage(filter.Column, filter.Operator))
            .When(x => x.Filters != null);
    }

    private static bool BeValidOperatorForColumn(string column, string operatorValue)
    {
        if (GuidColumns.Contains(column))
            return ValidGuidOperators.Contains(operatorValue);

        if (DateTimeColumns.Contains(column) || NumericColumns.Contains(column))
            return ValidComparisonOperators.Contains(operatorValue);

        return true;
    }

    private static string GetOperatorErrorMessage(string column, string operatorValue)
    {
        if (GuidColumns.Contains(column))
            return $"Column '{column}' is a Guid and only supports operators: {string.Join(", ", ValidGuidOperators)}.";

        if (DateTimeColumns.Contains(column))
            return $"Column '{column}' is a DateTime and only supports comparison operators: {string.Join(", ", ValidComparisonOperators)}.";

        if (NumericColumns.Contains(column))
            return $"Column '{column}' is numeric and only supports comparison operators: {string.Join(", ", ValidComparisonOperators)}.";

        return $"Invalid operator '{operatorValue}' for column '{column}'.";
    }
}
