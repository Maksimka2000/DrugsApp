using System;
using FluentValidation;

namespace DrugsApp.Features.PharmaceuticalFormFeature.CheckPharmaceuticalFormNameAvailability;

/// <summary>
/// Validator for CheckPharmaceuticalFormNameAvailability query parameters.
/// </summary>
public sealed class CheckPharmaceuticalFormNameAvailabilityRequestValidator
    : AbstractValidator<CheckPharmaceuticalFormNameAvailabilityRequest>
{
    public CheckPharmaceuticalFormNameAvailabilityRequestValidator()
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


