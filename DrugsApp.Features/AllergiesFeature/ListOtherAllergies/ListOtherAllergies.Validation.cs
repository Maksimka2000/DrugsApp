using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.ListOtherAllergies;

public sealed class ListOtherAllergiesRequestValidator : AbstractValidator<ListOtherAllergiesRequest>
{
    public ListOtherAllergiesRequestValidator()
    {
        RuleFor(r => r.Skip).GreaterThanOrEqualTo(0);
        RuleFor(r => r.Take).InclusiveBetween(1, 40);
    }
}
