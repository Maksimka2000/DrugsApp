using FluentValidation;

namespace DrugsApp.Features.PharmaceuticalFormFeature.AddPharmaceuticalForm;

/// <summary>
/// Validator for AddPharmaceuticalForm request payload.
/// </summary>
public sealed class AddPharmaceuticalFormRequestValidator : AbstractValidator<AddPharmaceuticalFormRequest>
{
    public AddPharmaceuticalFormRequestValidator()
    {
        RuleFor(x => x.FormulationName)
            .NotEmpty()
            .WithMessage("FormulationName is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("FormulationName must not be whitespace.")
            .MaximumLength(200)
            .WithMessage("FormulationName must not exceed 200 characters.");
    }
}


