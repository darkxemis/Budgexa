namespace Budgexa.Application.Tests.Customers.Commands.CreateCustomer;

using Budgexa.Application.Customers.Commands.CreateCustomer;
using Budgexa.Application.Customers.DTOs;
using FluentValidation.TestHelper;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _validator = new();

    private static CreateCustomerCommand Valid(
        string legalName = "Acme",
        string taxId = "B12345678",
        string? email = "billing@acme.test",
        string? tradeName = null,
        string? phone = null,
        string? addressLine = null,
        string? city = null,
        string? postalCode = null,
        string? province = null,
        string? country = null,
        string? notes = null)
    {
        return new CreateCustomerCommand(new CustomerCreateDto(
            legalName,
            tradeName,
            taxId,
            email,
            phone,
            addressLine,
            city,
            postalCode,
            province,
            country,
            notes));
    }

    [Fact]
    public void DefaultValid_HasNoErrors()
    {
        var result = _validator.TestValidate(Valid());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LegalName_WhenEmpty_HasError(string legalName)
    {
        var result = _validator.TestValidate(Valid(legalName: legalName));

        result.ShouldHaveValidationErrorFor(x => x.Dto.LegalName);
    }

    [Fact]
    public void LegalName_WhenTooLong_HasError()
    {
        var result = _validator.TestValidate(Valid(legalName: new string('a', 201)));

        result.ShouldHaveValidationErrorFor(x => x.Dto.LegalName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void TaxId_WhenEmpty_HasError(string taxId)
    {
        var result = _validator.TestValidate(Valid(taxId: taxId));

        result.ShouldHaveValidationErrorFor(x => x.Dto.TaxId);
    }

    [Fact]
    public void Email_WhenInvalidFormat_HasError()
    {
        var result = _validator.TestValidate(Valid(email: "not-an-email"));

        result.ShouldHaveValidationErrorFor(x => x.Dto.Email);
    }

    [Fact]
    public void Email_WhenNull_IsAllowed()
    {
        var result = _validator.TestValidate(Valid(email: null));

        result.ShouldNotHaveValidationErrorFor(x => x.Dto.Email);
    }

    [Fact]
    public void Phone_WhenTooLong_HasError()
    {
        var result = _validator.TestValidate(Valid(phone: new string('1', 51)));

        result.ShouldHaveValidationErrorFor(x => x.Dto.Phone);
    }

    [Fact]
    public void Notes_WhenTooLong_HasError()
    {
        var result = _validator.TestValidate(Valid(notes: new string('a', 2001)));

        result.ShouldHaveValidationErrorFor(x => x.Dto.Notes);
    }
}
