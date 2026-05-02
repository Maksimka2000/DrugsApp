using FluentValidation;

namespace DrugsApp.Features.SupplementFeature.SearchSupplements;

/// <summary>
/// Validator for SearchSupplements query parameters.
/// </summary>
public sealed class SearchSupplementsRequestValidator : AbstractValidator<SearchSupplementsRequest>
{
    public SearchSupplementsRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.SupplementName),
            () => RuleFor(request => request.SupplementName)
                .MaximumLength(256)
                .WithMessage("Supplement name filter must not exceed 256 characters."));

        RuleFor(request => request.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(request => request.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("Take must be between 1 and 40.");
    }
}
