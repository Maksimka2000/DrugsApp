using FluentValidation;

namespace DrugsApp.Features.SupplementFeature.CheckSupplementGroupNameAvailability;

/// <summary>
/// Validator for supplement group name availability query.
/// </summary>
public sealed class CheckSupplementGroupNameAvailabilityRequestValidator : AbstractValidator<CheckSupplementGroupNameAvailabilityRequest>
{
    public CheckSupplementGroupNameAvailabilityRequestValidator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be whitespace.")
            .MaximumLength(256)
            .WithMessage("Name must not exceed 256 characters.");

        When(request => request.ExcludeId is not null, () =>
        {
            RuleFor(request => request.ExcludeId)
                .Must(id => id != Guid.Empty)
                .WithMessage("ExcludeId must not be an empty Guid.");
        });
    }
}
