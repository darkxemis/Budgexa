namespace Budgexa.Application.Customers.Queries.GetCustomersGrid;

using FluentValidation;

public sealed class GetCustomersGridQueryValidator : AbstractValidator<GetCustomersGridQuery>
{
    public GetCustomersGridQueryValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Grid request is required.")
            .SetValidator(new Budgexa.Application.Customers.Validators.CustomerGridRequestDtoValidator());
    }
}
