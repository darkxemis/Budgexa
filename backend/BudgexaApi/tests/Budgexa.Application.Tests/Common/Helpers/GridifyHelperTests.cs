namespace Budgexa.Application.Tests.Common.Helpers;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Common.Helpers;

public class GridifyHelperTests
{
    [Fact]
    public void BuildFilterExpression_NullOrEmpty_ReturnsNull()
    {
        GridifyHelper.BuildFilterExpression(null).Should().BeNull();
        GridifyHelper.BuildFilterExpression(new List<GridFilterDto>()).Should().BeNull();
    }

    [Theory]
    [InlineData(GridifyOperators.Equal, "Name", "Alice", "Name=Alice")]
    [InlineData(GridifyOperators.NotEqual, "Name", "Alice", "Name!=Alice")]
    [InlineData(GridifyOperators.GreaterThan, "Age", "10", "Age>10")]
    [InlineData(GridifyOperators.GreaterThanOrEqual, "Age", "10", "Age>=10")]
    [InlineData(GridifyOperators.LessThan, "Age", "10", "Age<10")]
    [InlineData(GridifyOperators.LessThanOrEqual, "Age", "10", "Age<=10")]
    [InlineData(GridifyOperators.Contains, "Name", "li", "Name=*li")]
    [InlineData(GridifyOperators.NotContains, "Name", "li", "Name!*li")]
    [InlineData(GridifyOperators.StartsWith, "Name", "Al", "Name^Al")]
    [InlineData(GridifyOperators.NotStartsWith, "Name", "Al", "Name!^Al")]
    [InlineData(GridifyOperators.EndsWith, "Name", "ce", "Name$ce")]
    [InlineData(GridifyOperators.NotEndsWith, "Name", "ce", "Name!$ce")]
    public void BuildFilterExpression_KnownOperators_BuildsExpectedString(string op, string column, string value, string expected)
    {
        var result = GridifyHelper.BuildFilterExpression(new List<GridFilterDto>
        {
            new(column, op, value),
        });

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("equals", "Name=Alice")]
    [InlineData("notequals", "Name!=Alice")]
    [InlineData("contains", "Name=*Alice")]
    [InlineData("startswith", "Name^Alice")]
    [InlineData("endswith", "Name$Alice")]
    public void BuildFilterExpression_LegacyOperators_BuildsExpectedString(string op, string expected)
    {
        var result = GridifyHelper.BuildFilterExpression(new List<GridFilterDto>
        {
            new("Name", op, "Alice"),
        });

        result.Should().Be(expected);
    }

    [Fact]
    public void BuildFilterExpression_UnknownOperator_DefaultsToEquals()
    {
        var result = GridifyHelper.BuildFilterExpression(new List<GridFilterDto>
        {
            new("Name", "weird", "Alice"),
        });

        result.Should().Be("Name=Alice");
    }

    [Fact]
    public void BuildFilterExpression_SkipsEntriesWithEmptyColumn()
    {
        var result = GridifyHelper.BuildFilterExpression(new List<GridFilterDto>
        {
            new(string.Empty, GridifyOperators.Equal, "ignored"),
            new("Name", GridifyOperators.Equal, "Alice"),
        });

        result.Should().Be("Name=Alice");
    }

    [Fact]
    public void BuildFilterExpression_MultipleFilters_AreCommaJoined()
    {
        var result = GridifyHelper.BuildFilterExpression(new List<GridFilterDto>
        {
            new("Name", GridifyOperators.Equal, "Alice"),
            new("Age", GridifyOperators.GreaterThan, "21"),
        });

        result.Should().Be("Name=Alice,Age>21");
    }

    [Fact]
    public void BuildSortingExpression_NullOrEmpty_ReturnsNull()
    {
        GridifyHelper.BuildSortingExpression(null).Should().BeNull();
        GridifyHelper.BuildSortingExpression(new List<GridSortDto>()).Should().BeNull();
    }

    [Fact]
    public void BuildSortingExpression_BuildsAscAndDescColumns()
    {
        var result = GridifyHelper.BuildSortingExpression(new List<GridSortDto>
        {
            new("Name", IsDescending: false),
            new("Age", IsDescending: true),
        });

        result.Should().Be("Name asc, Age desc");
    }

    [Fact]
    public void BuildSortingExpression_SkipsEntriesWithEmptyColumn()
    {
        var result = GridifyHelper.BuildSortingExpression(new List<GridSortDto>
        {
            new(string.Empty, IsDescending: false),
            new("Name", IsDescending: true),
        });

        result.Should().Be("Name desc");
    }
}
