using FluentValidation;

namespace DrugsApp.Features.SupplementFeature.AddSupplement;

/// <summary>
/// Validator for AddSupplement request payload.
/// </summary>
public sealed class AddSupplementRequestValidator : AbstractValidator<AddSupplementRequest>
{
    public AddSupplementRequestValidator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name must not be null or whitespace.")
            .MaximumLength(256)
            .WithMessage("Name must not exceed 256 characters.");

        When(
            request => request.Aliases is not null && request.Aliases.Count > 0,
            () =>
            {
                RuleForEach(request => request.Aliases!)
                    .Must(alias => !string.IsNullOrWhiteSpace(alias))
                    .WithMessage("Alias must not be null or whitespace.")
                    .MaximumLength(256)
                    .WithMessage("Alias must not exceed 256 characters.");

                RuleFor(request => request.Aliases!)
                    .Must(aliases => aliases.Distinct(StringComparer.OrdinalIgnoreCase).Count() == aliases.Count)
                    .WithMessage("Aliases must be unique.");
            });
    }
}
