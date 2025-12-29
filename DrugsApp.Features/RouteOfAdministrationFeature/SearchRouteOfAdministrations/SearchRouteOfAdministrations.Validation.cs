using FluentValidation;

namespace DrugsApp.Features.RouteOfAdministrationFeature.SearchRouteOfAdministrations;

/// <summary>
/// Validator for SearchRouteOfAdministrations query parameters.
/// </summary>
public sealed class SearchRouteOfAdministrationsRequestValidator : AbstractValidator<SearchRouteOfAdministrationsRequest>
{
    public SearchRouteOfAdministrationsRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.RouteOfAdministrationName), () => RuleFor(x => x.RouteOfAdministrationName)
            .MaximumLength(200)
            .WithMessage("RouteOfAdministrationName filter must not exceed 200 characters."));

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 40)
            .WithMessage("Take must be between 1 and 40.");
    }
}


