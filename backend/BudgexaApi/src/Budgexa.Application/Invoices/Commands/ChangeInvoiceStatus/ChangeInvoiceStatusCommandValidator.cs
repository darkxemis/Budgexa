namespace Budgexa.Application.Invoices.Commands.ChangeInvoiceStatus;

using FluentValidation;

public sealed class ChangeInvoiceStatusCommandValidator : AbstractValidator<ChangeInvoiceStatusCommand>
{
    public ChangeInvoiceStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Invoice id is required.");
        RuleFor(x => x.Dto.StatusId).NotEmpty().WithMessage("Status id is required.");
    }
}
