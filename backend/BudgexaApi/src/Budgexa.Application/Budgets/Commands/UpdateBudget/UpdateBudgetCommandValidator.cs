namespace Budgexa.Application.Budgets.Commands.UpdateBudget;

using FluentValidation;

public sealed class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Budget id is required.");

        RuleFor(x => x.Dto.CustomerId)
            .NotEmpty().WithMessage("Customer id is required.");

        RuleFor(x => x.Dto.Number)
            .NotEmpty().WithMessage("Budget number is required.")
            .MaximumLength(50).WithMessage("Budget number cannot exceed 50 characters.");

        RuleFor(x => x.Dto.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.Dto.ValidUntil)
            .GreaterThanOrEqualTo(x => x.Dto.IssueDate)
            .WithMessage("Valid-until date cannot be before issue date.")
            .When(x => x.Dto.ValidUntil.HasValue);

        RuleFor(x => x.Dto.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Notes));

        RuleFor(x => x.Dto.TermsAndConditions)
            .MaximumLength(4000).WithMessage("Terms and conditions cannot exceed 4000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.TermsAndConditions));

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
        });
    }
}
