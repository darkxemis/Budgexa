namespace Budgexa.Application.Customers.Commands.UpdateCustomer;

using FluentValidation;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer id is required.");

        RuleFor(x => x.Dto.LegalName)
            .NotEmpty().WithMessage("Legal name is required.")
            .MaximumLength(200).WithMessage("Legal name cannot exceed 200 characters.");

        RuleFor(x => x.Dto.TradeName)
            .MaximumLength(200).WithMessage("Trade name cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.TradeName));

        RuleFor(x => x.Dto.TaxId)
            .NotEmpty().WithMessage("Tax id is required.")
            .MaximumLength(50).WithMessage("Tax id cannot exceed 50 characters.");

        RuleFor(x => x.Dto.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));

        RuleFor(x => x.Dto.Phone)
            .MaximumLength(50).WithMessage("Phone cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Phone));

        RuleFor(x => x.Dto.AddressLine)
            .MaximumLength(300).WithMessage("Address line cannot exceed 300 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.AddressLine));

        RuleFor(x => x.Dto.City)
            .MaximumLength(150).WithMessage("City cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.City));

        RuleFor(x => x.Dto.PostalCode)
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.PostalCode));

        RuleFor(x => x.Dto.Province)
            .MaximumLength(150).WithMessage("Province cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Province));

        RuleFor(x => x.Dto.Country)
            .MaximumLength(100).WithMessage("Country cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Country));

        RuleFor(x => x.Dto.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Notes));
    }
}
