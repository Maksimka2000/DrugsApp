using FluentValidation;

namespace DrugsApp.Features.FoodFeature.SearchFoodGroups;

/// <summary>
/// Validator for SearchFoodGroups query parameters.
/// </summary>
public sealed class SearchFoodGroupsRequestValidator : AbstractValidator<SearchFoodGroupsRequest>
{
    public SearchFoodGroupsRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.FoodNameQuery),
            () => RuleFor(request => request.FoodNameQuery)
                .MaximumLength(200)
                .WithMessage("Food name query must not exceed 200 characters."));

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("Take must be between 1 and 40.");
    }
}
