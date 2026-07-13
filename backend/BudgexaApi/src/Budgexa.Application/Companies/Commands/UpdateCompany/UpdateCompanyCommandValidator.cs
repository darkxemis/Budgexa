namespace Budgexa.Application.Companies.Commands.UpdateCompany;

using FluentValidation;

public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Dto.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Description));

        RuleFor(x => x.Dto.Phone)
            .MaximumLength(30).WithMessage("Phone cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Phone));

        RuleFor(x => x.Dto.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(200).WithMessage("Email cannot exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Dto.Email));
    }
}