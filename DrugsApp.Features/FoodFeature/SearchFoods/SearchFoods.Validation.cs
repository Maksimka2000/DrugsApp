using FluentValidation;

namespace DrugsApp.Features.FoodFeature.SearchFoods;

/// <summary>
/// Validator for SearchFoods query parameters.
/// </summary>
public sealed class SearchFoodsRequestValidator : AbstractValidator<SearchFoodsRequest>
{
    public SearchFoodsRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.FoodName),
            () => RuleFor(request => request.FoodName)
                .MaximumLength(200)
                .WithMessage("Food name filter must not exceed 200 characters."));

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("Take must be between 1 and 40.");
    }
}
