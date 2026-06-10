using FluentValidation;

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFood;

/// <summary>
/// Validates list regular food query parameters.
/// </summary>
public sealed class ListRegularFoodRequestValidator : AbstractValidator<ListRegularFoodRequest>
{
    public ListRegularFoodRequestValidator()
    {
        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);

        When(
            request => !string.IsNullOrWhiteSpace(request.FoodNameQuery),
            () =>
            {
                RuleFor(request => request.FoodNameQuery!)
                    .MaximumLength(200);
            });
    }
}
