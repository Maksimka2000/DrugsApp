using FluentValidation;

namespace DrugsApp.Features.PharmaceuticalFormFeature.UpdatePharmaceuticalForm;

/// <summary>
/// Validator for UpdatePharmaceuticalForm request payload.
/// </summary>
public sealed class UpdatePharmaceuticalFormRequestValidator : AbstractValidator<UpdatePharmaceuticalFormRequest>
{
    public UpdatePharmaceuticalFormRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.FormulationName), () => RuleFor(x => x.FormulationName)
            .MaximumLength(200)
            .WithMessage("FormulationName must not exceed 200 characters."));
    }
}


