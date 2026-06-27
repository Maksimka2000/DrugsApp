using FluentValidation;

namespace DrugsApp.Features.SupportFeature.UpdateFaqEntry;

/// <summary>
/// Validator for the FAQ patch request payload.
/// </summary>
public sealed class UpdateFaqEntryRequestValidator : AbstractValidator<UpdateFaqEntryRequest>
{
    public UpdateFaqEntryRequestValidator()
    {
        When(
            request => request.En is not null,
            () =>
            {
                RuleFor(request => request.En!)
                    .SetValidator(new UpdateFaqTranslationRequestValidator());
            });

        When(
            request => request.Pl is not null,
            () =>
            {
                RuleFor(request => request.Pl!)
                    .SetValidator(new UpdateFaqTranslationRequestValidator());
            });

        RuleFor(request => request)
            .Must(request => request.En is not null || request.Pl is not null)
            .WithMessage("At least one translation must be provided.");
    }
}

/// <summary>
/// Validator for a partial localized FAQ translation update.
/// </summary>
public sealed class UpdateFaqTranslationRequestValidator : AbstractValidator<UpdateFaqTranslationRequest>
{
    public UpdateFaqTranslationRequestValidator()
    {
        When(
            request => request.Question is not null,
            () =>
            {
                RuleFor(request => request.Question)
                    .Must(question => !string.IsNullOrWhiteSpace(question))
                    .WithMessage("Question must not be empty or whitespace.")
                    .MaximumLength(300)
                    .WithMessage("Question must not exceed 300 characters.");
            });

        When(
            request => request.Answer is not null,
            () =>
            {
                RuleFor(request => request.Answer)
                    .Must(answer => !string.IsNullOrWhiteSpace(answer))
                    .WithMessage("Answer must not be empty or whitespace.");
            });

        RuleFor(request => request)
            .Must(request => request.Question is not null || request.Answer is not null)
            .WithMessage("At least one field must be provided for the translation.");
    }
}
