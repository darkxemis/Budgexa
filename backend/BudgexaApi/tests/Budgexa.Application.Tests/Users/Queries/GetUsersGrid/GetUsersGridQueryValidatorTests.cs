namespace Budgexa.Application.Tests.Users.Queries.GetUsersGrid;

using Budgexa.Application.Common.DTOs;
using Budgexa.Application.Users.Queries.GetUsersGrid;
using FluentValidation.TestHelper;

public class GetUsersGridQueryValidatorTests
{
    private readonly GetUsersGridQueryValidator _validator = new();

    [Fact]
    public void Valid_HasNoErrors()
    {
        var query = new GetUsersGridQuery(new GridRequestDto(1, 10, null, null, null));

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidNestedDto_PropagatesError()
    {
        var query = new GetUsersGridQuery(new GridRequestDto(0, 10, null, null, null));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor("Request.Page");
    }
}
