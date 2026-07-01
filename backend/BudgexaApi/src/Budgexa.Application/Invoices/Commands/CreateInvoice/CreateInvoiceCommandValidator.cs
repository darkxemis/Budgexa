namespace Budgexa.Application.Invoices.Commands.CreateInvoice;

using FluentValidation;

public sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.Dto.CustomerId)
            .NotEmpty().WithMessage("Customer id is required.");

        RuleFor(x => x.Dto.Series)
            .NotEmpty().WithMessage("Series is required.")
            .MaximumLength(20).WithMessage("Series cannot exceed 20 characters.");

        RuleFor(x => x.Dto.Number)
            .NotEmpty().WithMessage("Invoice number is required.")
            .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters.");

        RuleFor(x => x.Dto.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.Dto.DueDate)
            .GreaterThanOrEqualTo(x => x.Dto.IssueDate)
            .WithMessage("Due date cannot be before issue date.");

        RuleFor(x => x.Dto.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Notes));

        RuleForEach(x => x.Dto.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Description)
                .NotEmpty().WithMessage("Line description is required.")
                .MaximumLength(500).WithMessage("Line description cannot exceed 500 characters.");

            line.RuleFor(l => l.Unit)
                .NotEmpty().WithMessage("Line unit is required.")
                .MaximumLength(50).WithMessage("Line unit cannot exceed 50 characters.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0).WithMessage("Line quantity must be greater than zero.");

            line.RuleFor(l => l.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Line unit price cannot be negative.");

            line.RuleFor(l => l.DiscountPercentage)
                .InclusiveBetween(0, 100).WithMessage("Line discount must be between 0 and 100.");

            line.RuleFor(l => l.TaxRate)
                .InclusiveBetween(0, 100).WithMessage("Line tax rate must be between 0 and 100.");

            line.RuleFor(l => l.WithholdingRate)
                .InclusiveBetween(0, 100).WithMessage("Line withholding rate must be between 0 and 100.");
        });
    }
}
