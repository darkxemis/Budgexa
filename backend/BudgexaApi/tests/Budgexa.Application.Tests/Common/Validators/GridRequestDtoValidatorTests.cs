namespace Budgexa.Application.Tests.Common.Validators;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Validators;
using FluentValidation.TestHelper;

public class GridRequestDtoValidatorTests
{
    private readonly GridRequestDtoValidator _validator = new();

    private static GridRequestDto Valid(
        int page = 1,
        int pageSize = 10,
        List<GridSortDto>? sorting = null,
        List<GridFilterDto>? filters = null,
        string? search = null) =>
        new(page, pageSize, sorting, filters, search);

    [Fact]
    public void DefaultValid_HasNoErrors()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Page_WhenNotPositive_HasError(int page)
    {
        var result = _validator.TestValidate(Valid(page: page));

        result.ShouldHaveValidationErrorFor(x => x.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void PageSize_WhenInvalid_HasError(int pageSize)
    {
        var result = _validator.TestValidate(Valid(pageSize: pageSize));

        result.ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [Fact]
    public void Search_WhenTooLong_HasError()
    {
        var result = _validator.TestValidate(Valid(search: new string('a', 501)));

        result.ShouldHaveValidationErrorFor(x => x.Search);
    }

    [Fact]
    public void Filters_WhenExceedingMax_HasError()
    {
        var filters = Enumerable.Range(0, 51)
            .Select(i => new GridFilterDto("Column" + i, GridifyOperators.Equal, "value"))
            .ToList();

        var result = _validator.TestValidate(Valid(filters: filters));

        result.ShouldHaveValidationErrorFor(x => x.Filters);
    }

    [Fact]
    public void Filter_Column_MustBeAlphanumericStartingWithLetter()
    {
        var filters = new List<GridFilterDto> { new("1column", GridifyOperators.Equal, "val") };

        var result = _validator.TestValidate(Valid(filters: filters));

        result.ShouldHaveValidationErrorFor("Filters[0].Column");
    }

    [Fact]
    public void Filter_Operator_MustBeKnown()
    {
        var filters = new List<GridFilterDto> { new("Email", "unknown", "val") };

        var result = _validator.TestValidate(Valid(filters: filters));

        result.ShouldHaveValidationErrorFor("Filters[0].Operator");
    }

    [Fact]
    public void Filter_Operator_BackwardsCompatibleNames_AreAccepted()
    {
        var filters = new List<GridFilterDto> { new("Email", "contains", "val") };

        var result = _validator.TestValidate(Valid(filters: filters));

        result.ShouldNotHaveValidationErrorFor("Filters[0].Operator");
    }

    [Fact]
    public void Sorting_WhenExceedingMax_HasError()
    {
        var sorting = Enumerable.Range(0, 11)
            .Select(i => new GridSortDto("Column" + i, false))
            .ToList();

        var result = _validator.TestValidate(Valid(sorting: sorting));

        result.ShouldHaveValidationErrorFor(x => x.Sorting);
    }

    [Fact]
    public void Sort_Column_MustBeAlphanumericStartingWithLetter()
    {
        var sorting = new List<GridSortDto> { new("1column", false) };

        var result = _validator.TestValidate(Valid(sorting: sorting));

        result.ShouldHaveValidationErrorFor("Sorting[0].Column");
    }
}
