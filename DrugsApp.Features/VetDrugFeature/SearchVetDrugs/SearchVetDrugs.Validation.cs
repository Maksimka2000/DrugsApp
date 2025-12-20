using FluentValidation;

namespace DrugsApp.Features.VetDrugFeature.SearchVetDrugs;



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

        RuleFor(x => x.Order)
                .NotEmpty()
                .Must(order => order!.Equals("ascending", System.StringComparison.OrdinalIgnoreCase) ||
                              order.Equals("descending", System.StringComparison.OrdinalIgnoreCase))
                .WithMessage("Order must be either 'ascending' or 'descending'.");

        RuleFor(x => x.Skip)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(x => x.Take)
                .InclusiveBetween(1, 100)
                .WithMessage("Take must be between 1 and 100.");
    }
}

