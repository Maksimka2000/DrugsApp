using FluentValidation;

namespace DrugsApp.Features.SupplementFeature.AddSupplementGroup;

/// <summary>
/// Validator for AddSupplementGroup request payload.
/// </summary>
public sealed class AddSupplementGroupRequestValidator : AbstractValidator<AddSupplementGroupRequest>
{
    public AddSupplementGroupRequestValidator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be null or whitespace.")
            .MaximumLength(256)
            .WithMessage("Name must not exceed 256 characters.");

        When(
            request => request.SupplementIds is not null && request.SupplementIds.Count > 0,
            () => RuleFor(request => request.SupplementIds!)
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("All supplement ids must be valid GUIDs.")
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate supplement ids are not allowed."));

    }
}
