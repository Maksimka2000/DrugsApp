using DrugsApp.Domain.SupportFeature;
using DrugsApp.Domain.SupportFeature.Enums;
using FluentValidation;

namespace DrugsApp.Features.SupportFeature.SearchPublicFaqEntries;

/// <summary>
/// Validator for the public FAQ search request payload.
/// </summary>
public sealed class SearchPublicFaqEntriesRequestValidator : AbstractValidator<SearchPublicFaqEntriesRequest>
{
    public SearchPublicFaqEntriesRequestValidator()
    {
        RuleFor(request => request.Language)
            .NotEmpty()
            .WithMessage("Language is required.")
            .Must(language => FaqLanguageCodeExtensions.TryParseCode(language, out _))
            .WithMessage("Language must be either 'en' or 'pl'.");

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("Take must be between 1 and 40.");
    }
}
