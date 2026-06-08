using FluentValidation;

namespace DrugsApp.Features.SupportFeature.SearchPublicFaqEntries;

/// <summary>
/// Validator for public FAQ query parameters.
/// </summary>
public sealed class SearchPublicFaqEntriesRequestValidator : AbstractValidator<SearchPublicFaqEntriesRequest>
{
    public SearchPublicFaqEntriesRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.QuestionQuery),
            () =>
            {
                RuleFor(request => request.QuestionQuery)
                    .MaximumLength(200)
                    .WithMessage("question_query must not exceed 200 characters.");
            });

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("take must be between 1 and 40.");
    }
}
