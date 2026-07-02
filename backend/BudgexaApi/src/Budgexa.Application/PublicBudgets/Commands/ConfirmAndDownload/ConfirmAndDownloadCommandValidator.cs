namespace Budgexa.Application.PublicBudgets.Commands.ConfirmAndDownload;

using FluentValidation;

public sealed class ConfirmAndDownloadCommandValidator : AbstractValidator<ConfirmAndDownloadCommand>
{
    public ConfirmAndDownloadCommandValidator()
    {
        RuleFor(x => x.Request.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.");

        RuleFor(x => x.Request.CustomerFirstName)
            .NotEmpty()
            .WithMessage("Customer first name is required.")
            .MaximumLength(100)
            .WithMessage("Customer first name must not exceed 100 characters.");

        RuleFor(x => x.Request.CustomerLastName)
            .NotEmpty()
            .WithMessage("Customer last name is required.")
            .MaximumLength(100)
            .WithMessage("Customer last name must not exceed 100 characters.");

        RuleFor(x => x.Request.Lines)
            .NotEmpty()
            .WithMessage("At least one line item is required.");

        RuleForEach(x => x.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ItemId)
                .NotEmpty()
                .WithMessage("ItemId is required.");

            line.RuleFor(l => l.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(10000)
                .WithMessage("Quantity must not exceed 10,000.");
        });
    }
}
