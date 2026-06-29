namespace Budgexa.Application.Tests.Users.Validators;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Users.Validators;
using FluentValidation.TestHelper;

public class UserGridRequestDtoValidatorTests
{
    private readonly UserGridRequestDtoValidator _validator = new();

    private static GridRequestDto WithFilter(string column, string op) =>
        new(1, 10, null, new List<GridFilterDto> { new(column, op, "value") }, null);

    [Fact]
    public void GuidColumn_WithEqualOperator_IsValid()
    {
        var result = _validator.TestValidate(WithFilter("Id", GridifyOperators.Equal));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void GuidColumn_WithGreaterThanOperator_HasError()
    {
        var result = _validator.TestValidate(WithFilter("CompanyId", GridifyOperators.GreaterThan));

        result.ShouldHaveValidationErrorFor("Filters[0]");
    }

    [Fact]
    public void DateTimeColumn_WithGreaterThanOrEqual_IsValid()
    {
        var result = _validator.TestValidate(WithFilter("CreatedAt", GridifyOperators.GreaterThanOrEqual));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DateTimeColumn_WithContains_HasError()
    {
        var result = _validator.TestValidate(WithFilter("UpdatedAt", GridifyOperators.Contains));

        result.ShouldHaveValidationErrorFor("Filters[0]");
    }

    [Fact]
    public void StringColumn_WithAnyValidOperator_IsValid()
    {
        var result = _validator.TestValidate(WithFilter("Email", GridifyOperators.Contains));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IncludesGridRequestDtoValidator_BasePageValidation()
    {
        var dto = new GridRequestDto(0, 10, null, null, null);

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }
}
