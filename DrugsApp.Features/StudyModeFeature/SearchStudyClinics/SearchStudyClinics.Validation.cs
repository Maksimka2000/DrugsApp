using FluentValidation;

namespace DrugsApp.Features.StudyModeFeature.SearchStudyClinics;

/// <summary>
/// Validator for study clinic search query parameters.
/// </summary>
public sealed class SearchStudyClinicsRequestValidator : AbstractValidator<SearchStudyClinicsRequest>
{
    /// <summary>
    /// Initializes search, skip and take rules.
    /// </summary>
    public SearchStudyClinicsRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.Search),
            () =>
            {
                RuleFor(request => request.Search)
                    .MaximumLength(120)
                    .WithMessage("search must not exceed 120 characters.");
            });

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("take must be between 1 and 40.");
    }
}
