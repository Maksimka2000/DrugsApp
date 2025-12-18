using FluentValidation;

namespace DrugsApp.Features.VetDrugFeature.SearchVetDrugs;

/// <summary>
/// Request model for search query parameters.
/// </summary>
public sealed record SearchVetDrugsRequest(
    string? Name,
    string? Order,
    int? Skip,
    int? Take);

/// <summary>
/// Validator for SearchVetDrugs query parameters.
/// </summary>
public sealed class SearchVetDrugsRequestValidator : AbstractValidator<SearchVetDrugsRequest>
{
    public SearchVetDrugsRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name)
                .MaximumLength(300)
                .WithMessage("Name filter must not exceed 300 characters."));

        When(x => !string.IsNullOrWhiteSpace(x.Order), () => RuleFor(x => x.Order)
                .Must(order => order!.Equals("ascending", System.StringComparison.OrdinalIgnoreCase) ||
                              order.Equals("descending", System.StringComparison.OrdinalIgnoreCase))
                .WithMessage("Order must be either 'ascending' or 'descending'."));

        When(x => x.Skip.HasValue, () => RuleFor(x => x.Skip)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Skip must be greater than or equal to 0."));

        When(x => x.Take.HasValue, () => RuleFor(x => x.Take)
                .InclusiveBetween(1, 100)
                .WithMessage("Take must be between 1 and 100."));
    }
}

