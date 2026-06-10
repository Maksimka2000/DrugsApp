using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.ListSupplementAllergies;

public sealed class ListSupplementAllergiesRequestValidator : AbstractValidator<ListSupplementAllergiesRequest>
{
    public ListSupplementAllergiesRequestValidator()
    {
        RuleFor(r => r.Skip).GreaterThanOrEqualTo(0);
        RuleFor(r => r.Take).InclusiveBetween(1, 40);
    }
}
