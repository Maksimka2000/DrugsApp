using FluentValidation;

namespace DrugsApp.Features.RegularFoodFeature.ListRegularFoodHistory;

/// <summary>
/// Validates list regular food history query parameters.
/// </summary>
public sealed class ListRegularFoodHistoryRequestValidator : AbstractValidator<ListRegularFoodHistoryRequest>
{
    public ListRegularFoodHistoryRequestValidator()
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
