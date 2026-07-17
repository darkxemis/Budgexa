namespace Budgexa.Application.PublicBudgets.Queries.GeneratePublicBudgetWithAi;

using Budgexa.Application.PublicBudgets.DTOs;
using FluentValidation;

public sealed class GeneratePublicBudgetWithAiQueryValidator : AbstractValidator<GeneratePublicBudgetWithAiQuery>
{
    public GeneratePublicBudgetWithAiQueryValidator()
    {
        RuleFor(x => x.Request.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required.");

        RuleFor(x => x.Request.UserRequest)
            .NotEmpty()
            .WithMessage("User request text is required.")
            .MaximumLength(2000)
            .WithMessage("User request text must not exceed 2000 characters.");
    }
}
