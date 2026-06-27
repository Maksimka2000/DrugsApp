using FluentValidation;

namespace DrugsApp.Features.SupportFeature.Shared;

/// <summary>
/// Validator for localized FAQ question and answer content.
/// </summary>
public sealed class FaqTranslationContentDtoValidator : AbstractValidator<FaqTranslationContentDto>
{
    /// <summary>
    /// Initializes validation rules for FAQ translation content.
    /// </summary>
    public FaqTranslationContentDtoValidator()
    {
        RuleFor(content => content.Question)
            .NotEmpty()
            .WithMessage("Question is required.")
            .Must(question => !string.IsNullOrWhiteSpace(question))
            .WithMessage("Question must not be whitespace.")
            .MaximumLength(300)
            .WithMessage("Question must not exceed 300 characters.");

        RuleFor(content => content.Answer)
            .NotEmpty()
            .WithMessage("Answer is required.")
            .Must(answer => !string.IsNullOrWhiteSpace(answer))
            .WithMessage("Answer must not be whitespace.");
    }
}
