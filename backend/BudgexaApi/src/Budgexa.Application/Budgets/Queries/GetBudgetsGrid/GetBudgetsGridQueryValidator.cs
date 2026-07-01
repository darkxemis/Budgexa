namespace Budgexa.Application.Budgets.Queries.GetBudgetsGrid;

using FluentValidation;

public sealed class GetBudgetsGridQueryValidator : AbstractValidator<GetBudgetsGridQuery>
{
    public GetBudgetsGridQueryValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Grid request is required.")
            .SetValidator(new Budgexa.Application.Budgets.Validators.BudgetGridRequestDtoValidator());
    }
}
