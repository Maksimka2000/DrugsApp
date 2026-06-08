using FluentValidation;

namespace DrugsApp.Features.SupportFeature.AddFaqEntry;

/// <summary>
/// Validator for the FAQ create request payload.
/// </summary>
public sealed class AddFaqEntryRequestValidator : AbstractValidator<AddFaqEntryRequest>
{
    public AddFaqEntryRequestValidator()
    {
        RuleFor(request => request.Question)
            .NotEmpty()
            .WithMessage("Question is required.")
            .Must(question => !string.IsNullOrWhiteSpace(question))
            .WithMessage("Question must not be whitespace.")
            .MaximumLength(300)
            .WithMessage("Question must not exceed 300 characters.");

        RuleFor(request => request.Answer)
            .NotEmpty()
            .WithMessage("Answer is required.")
            .Must(answer => !string.IsNullOrWhiteSpace(answer))
            .WithMessage("Answer must not be whitespace.");
    }
}
