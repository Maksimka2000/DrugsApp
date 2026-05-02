using FluentValidation;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplement;

/// <summary>
/// Validator for UpdateSupplement request payload.
/// </summary>
public sealed class UpdateSupplementRequestValidator : AbstractValidator<UpdateSupplementRequest>
{
    public UpdateSupplementRequestValidator()
    {
        When(
            request => !string.IsNullOrWhiteSpace(request.Name),
            () => RuleFor(request => request.Name)
                .MaximumLength(256)
                .WithMessage("Name must not exceed 256 characters."));

        When(
            request => request.Aliases is not null && request.Aliases.Count > 0,
            () =>
            {
                RuleFor(request => request.Aliases!)
                    .Must(aliases => aliases.Distinct(StringComparer.OrdinalIgnoreCase).Count() == aliases.Count)
                    .WithMessage("Aliases must be unique.");

                RuleForEach(request => request.Aliases!)
                   .Must(alias => !string.IsNullOrWhiteSpace(alias))
                   .WithMessage("Alias must not be null or whitespace.")
                   .MaximumLength(256)
                   .WithMessage("Alias must not exceed 256 characters.");
            });
    }
}
