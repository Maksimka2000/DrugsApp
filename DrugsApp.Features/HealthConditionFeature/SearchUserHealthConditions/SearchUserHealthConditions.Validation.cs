using FluentValidation;

namespace DrugsApp.Features.HealthConditionFeature.SearchUserHealthConditions;

/// <summary>
/// Validator for searching user health conditions.
/// </summary>
public sealed class SearchUserHealthConditionsRequestValidator : AbstractValidator<SearchUserHealthConditionsRequest>
{
    public SearchUserHealthConditionsRequestValidator()
    {
        RuleFor(request => request.DisplayNameQuery)
            .MaximumLength(256);

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
