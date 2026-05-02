using FluentValidation;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;

/// <summary>
/// Validator for UpdateSupplementGroup request payload.
/// </summary>
public sealed class UpdateSupplementGroupRequestValidator : AbstractValidator<UpdateSupplementGroupRequest>
{
    public UpdateSupplementGroupRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.Name),
            () => RuleFor(request => request.Name)
                .MaximumLength(256)
                .WithMessage("Name must not exceed 256 characters."));

        When(
            request => request.SupplementIds is not null && request.SupplementIds.Count > 0,
            () =>
            {
                RuleFor(request => request.SupplementIds!)
                    .Must(ids => ids.All(id => id != Guid.Empty))
                    .WithMessage("Supplement ids cannot be empty when provided.")
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("Duplicate supplement ids are not allowed.");
            });
    }
}
