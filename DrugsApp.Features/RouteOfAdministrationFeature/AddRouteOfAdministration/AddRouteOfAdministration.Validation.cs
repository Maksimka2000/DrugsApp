using FluentValidation;

namespace DrugsApp.Features.RouteOfAdministrationFeature.AddRouteOfAdministration;

/// <summary>
/// Validator for AddRouteOfAdministration request payload.
/// </summary>
public sealed class AddRouteOfAdministrationRequestValidator : AbstractValidator<AddRouteOfAdministrationRequest>
{
    public AddRouteOfAdministrationRequestValidator()
    {
        RuleFor(x => x.RouteOfAdministrationName)
            .NotEmpty()
            .WithMessage("RouteOfAdministrationName is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("RouteOfAdministrationName must not be whitespace.")
            .MaximumLength(200)
            .WithMessage("RouteOfAdministrationName must not exceed 200 characters.");
    }
}


