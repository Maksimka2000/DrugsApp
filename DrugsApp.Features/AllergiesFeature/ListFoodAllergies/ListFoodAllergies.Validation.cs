using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.ListFoodAllergies;

public sealed class ListFoodAllergiesRequestValidator : AbstractValidator<ListFoodAllergiesRequest>
{
    public ListFoodAllergiesRequestValidator()
    {
        RuleFor(r => r.Skip).GreaterThanOrEqualTo(0);
        RuleFor(r => r.Take).InclusiveBetween(1, 40);
    }
}
