namespace Budgexa.Application.Users.Queries.GetUsersGrid;

using Budgexa.Application.Users.Validators;
using FluentValidation;

public sealed class GetUsersGridQueryValidator : AbstractValidator<GetUsersGridQuery>
{
    public GetUsersGridQueryValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Request cannot be null.")
            .SetValidator(new UserGridRequestDtoValidator());
    }
}
