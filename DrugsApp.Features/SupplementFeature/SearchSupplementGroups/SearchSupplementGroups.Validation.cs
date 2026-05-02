using FluentValidation;

namespace DrugsApp.Features.SupplementFeature.SearchSupplementGroups;

/// <summary>
/// Validator for search supplement groups query parameters.
/// </summary>
public sealed class SearchSupplementGroupsRequestValidator : AbstractValidator<SearchSupplementGroupsRequest>
{
    public SearchSupplementGroupsRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.SupplementNameQuery),
            () => RuleFor(request => request.SupplementNameQuery)
                .MaximumLength(256)
                .WithMessage("Supplement name query must not exceed 256 characters."));

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("Take must be between 1 and 40.");
    }
}
