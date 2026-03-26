using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceFeature.SearchActiveSubstances;

/// <summary>
/// Validator for SearchActiveSubstances query parameters.
/// </summary>
public sealed class SearchActiveSubstancesRequestValidator : AbstractValidator<SearchActiveSubstancesRequest>
{
    public SearchActiveSubstancesRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.ActiveSubstanceName), () => RuleFor(x => x.ActiveSubstanceName)
            .MaximumLength(200)
            .WithMessage("ActiveSubstanceName filter must not exceed 200 characters."));

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("Take must be between 1 and 40.");
    }
}


