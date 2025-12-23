using FluentValidation;

namespace DrugsApp.Features.RouteOfAdministrationFeature.UpdateRouteOfAdministration;

/// <summary>
/// Validator for UpdateRouteOfAdministration request payload.
/// </summary>
public sealed class UpdateRouteOfAdministrationRequestValidator : AbstractValidator<UpdateRouteOfAdministrationRequest>
{
    public UpdateRouteOfAdministrationRequestValidator()
    {
        When(x => x.RouteOfAdministrationName is not null, () =>
        {
            RuleFor(x => x.RouteOfAdministrationName)
                .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("RouteOfAdministrationName must not be whitespace.");

            When(x => !string.IsNullOrWhiteSpace(x.RouteOfAdministrationName), () => RuleFor(x => x.RouteOfAdministrationName)
                .MaximumLength(200)
                .WithMessage("RouteOfAdministrationName must not exceed 200 characters."));
        });
    }
}


