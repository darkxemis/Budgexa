namespace Budgexa.Application.Budgets.Commands.ChangeBudgetStatus;

using FluentValidation;

public sealed class ChangeBudgetStatusCommandValidator : AbstractValidator<ChangeBudgetStatusCommand>
{
    public ChangeBudgetStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Budget id is required.");
        RuleFor(x => x.Dto.StatusId).NotEmpty().WithMessage("Status id is required.");
    }
}
