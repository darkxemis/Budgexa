namespace Budgexa.Application.Users.Validators;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Validators;
using FluentValidation;

public sealed class UserGridRequestDtoValidator : AbstractValidator<GridRequestDto>
{
    private static readonly HashSet<string> GuidColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CompanyId", "LanguageId", "StatusId"
    };

    private static readonly HashSet<string> DateTimeColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreatedAt", "UpdatedAt"
    };

    private static readonly HashSet<string> StringColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email", "FirstName", "LastName", "CompanyName", "LanguageName", "StatusName"
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

    public UserGridRequestDtoValidator()
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
        {
            return ValidGuidOperators.Contains(operatorValue);
        }

        if (DateTimeColumns.Contains(column))
        {
            return ValidComparisonOperators.Contains(operatorValue);
        }

        return true;
    }

    private static string GetOperatorErrorMessage(string column, string operatorValue)
    {
        if (GuidColumns.Contains(column))
        {
            return $"Column '{column}' is a Guid and only supports operators: {string.Join(", ", ValidGuidOperators)}.";
        }

        if (DateTimeColumns.Contains(column))
        {
            return $"Column '{column}' is a DateTime and only supports comparison operators: {string.Join(", ", ValidComparisonOperators)}.";
        }

        return $"Invalid operator '{operatorValue}' for column '{column}'.";
    }
}
