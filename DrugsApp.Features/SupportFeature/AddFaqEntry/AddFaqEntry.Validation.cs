using FluentValidation;
using DrugsApp.Features.SupportFeature.Shared;

namespace DrugsApp.Features.SupportFeature.AddFaqEntry;

/// <summary>
/// Validator for the FAQ create request payload.
/// </summary>
public sealed class AddFaqEntryRequestValidator : AbstractValidator<AddFaqEntryRequest>
{
    public AddFaqEntryRequestValidator()
    {
        RuleFor(request => request.En)
            .NotNull()
            .WithMessage("English translation is required.")
            .SetValidator(new FaqTranslationContentDtoValidator());

        RuleFor(request => request.Pl)
            .NotNull()
            .WithMessage("Polish translation is required.")
            .SetValidator(new FaqTranslationContentDtoValidator());
    }
}
