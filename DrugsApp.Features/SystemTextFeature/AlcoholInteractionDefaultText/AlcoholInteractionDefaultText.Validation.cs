using FluentValidation;

namespace DrugsApp.Features.SystemTextFeature.AlcoholInteractionDefaultText;

/// <summary>
/// Validator for updating the default alcohol interaction text.
/// </summary>
public sealed class UpdateAlcoholInteractionDefaultTextRequestValidator
    : AbstractValidator<UpdateAlcoholInteractionDefaultTextRequest>
{
    public UpdateAlcoholInteractionDefaultTextRequestValidator()
    {
        RuleFor(request => request.DefaultText)
            .NotEmpty()
            .WithMessage("Default text must be provided.")
            .MaximumLength(1000)
            .WithMessage("Default text must not exceed 1000 characters.")
            .Must(text => !string.IsNullOrWhiteSpace(text))
            .WithMessage("Default text must not be whitespace only.");
    }
}
