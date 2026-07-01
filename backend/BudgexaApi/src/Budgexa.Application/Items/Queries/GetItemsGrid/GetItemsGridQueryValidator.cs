namespace Budgexa.Application.Items.Queries.GetItemsGrid;

using FluentValidation;

public sealed class GetItemsGridQueryValidator : AbstractValidator<GetItemsGridQuery>
{
    public GetItemsGridQueryValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Grid request is required.")
            .SetValidator(new Budgexa.Application.Items.Validators.ItemGridRequestDtoValidator());
    }
}
