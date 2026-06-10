using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.SearchHealthConditions;

/// <summary>
/// Validates search query parameters for health conditions.
/// </summary>
public sealed class SearchHealthConditionsRequestValidator : AbstractValidator<SearchHealthConditionsRequest>
{
    public SearchHealthConditionsRequestValidator()
    {
        RuleFor(request => request.HealthConditionNameQuery)
            .MaximumLength(256);

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
