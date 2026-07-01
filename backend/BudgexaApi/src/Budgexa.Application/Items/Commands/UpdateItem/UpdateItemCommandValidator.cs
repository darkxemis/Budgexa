namespace Budgexa.Application.Items.Commands.UpdateItem;

using FluentValidation;

public sealed class UpdateItemCommandValidator : AbstractValidator<UpdateItemCommand>
{
    public UpdateItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Item id is required.");

        RuleFor(x => x.Dto.Sku)
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Sku));

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Description));

        RuleFor(x => x.Dto.Type)
            .IsInEnum().WithMessage("Item type must be a valid value.");

        RuleFor(x => x.Dto.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(50).WithMessage("Unit cannot exceed 50 characters.");

        RuleFor(x => x.Dto.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

        RuleFor(x => x.Dto.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100.");

        RuleFor(x => x.Dto.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");
    }
}
