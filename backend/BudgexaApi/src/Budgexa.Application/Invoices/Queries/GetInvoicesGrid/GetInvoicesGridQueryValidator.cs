namespace Budgexa.Application.Invoices.Queries.GetInvoicesGrid;

using Budgexa.Application.Invoices.Validators;
using FluentValidation;

public sealed class GetInvoicesGridQueryValidator : AbstractValidator<GetInvoicesGridQuery>
{
    public GetInvoicesGridQueryValidator()
    {
        RuleFor(x => x.Request).NotNull().SetValidator(new InvoiceGridRequestDtoValidator());
    }
}
