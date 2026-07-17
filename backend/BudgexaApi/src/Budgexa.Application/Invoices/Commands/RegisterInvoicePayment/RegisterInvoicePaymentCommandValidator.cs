namespace Budgexa.Application.Invoices.Commands.RegisterInvoicePayment;

using FluentValidation;

public sealed class RegisterInvoicePaymentCommandValidator : AbstractValidator<RegisterInvoicePaymentCommand>
{
    public RegisterInvoicePaymentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Invoice id is required.");

        RuleFor(x => x.Dto.Amount)
            .GreaterThan(0).WithMessage("Payment amount must be greater than zero.");

        RuleFor(x => x.Dto.Method)
            .IsInEnum().WithMessage("Payment method must be a valid value.");

        RuleFor(x => x.Dto.Reference)
            .MaximumLength(200).WithMessage("Payment reference cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Reference));
    }
}
