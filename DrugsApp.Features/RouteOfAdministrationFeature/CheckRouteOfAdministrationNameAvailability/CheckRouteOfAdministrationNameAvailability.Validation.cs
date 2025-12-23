using System;
using FluentValidation;

namespace DrugsApp.Features.RouteOfAdministrationFeature.CheckRouteOfAdministrationNameAvailability;

/// <summary>
/// Validator for CheckRouteOfAdministrationNameAvailability query parameters.
/// </summary>
public sealed class CheckRouteOfAdministrationNameAvailabilityRequestValidator
    : AbstractValidator<CheckRouteOfAdministrationNameAvailabilityRequest>
{
    public CheckRouteOfAdministrationNameAvailabilityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be whitespace.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");

        When(x => x.ExcludeId is not null, () => RuleFor(x => x.ExcludeId)
            .NotEqual(Guid.Empty)
            .WithMessage("ExcludeId must not be an empty Guid."));
    }
}


