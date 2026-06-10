using FluentValidation;

namespace DrugsApp.Features.AllergiesFeature.ListDrugAllergies;

/// <summary>
/// Validator for listing drug allergies.
/// </summary>
public sealed class ListDrugAllergiesRequestValidator : AbstractValidator<ListDrugAllergiesRequest>
{
    public ListDrugAllergiesRequestValidator()
    {
        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40);
    }
}
