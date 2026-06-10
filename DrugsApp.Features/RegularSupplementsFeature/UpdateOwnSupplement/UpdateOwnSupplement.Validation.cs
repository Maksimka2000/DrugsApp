using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateOwnSupplement;

/// <summary>
/// Validates update own supplement request payloads.
/// </summary>
public sealed class UpdateOwnSupplementRequestValidator : AbstractValidator<UpdateOwnSupplementRequest>
{
    public UpdateOwnSupplementRequestValidator()
    {
        RuleFor(request => request.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("name must not be empty.");

        RuleFor(request => request.SupplementIds)
            .NotNull()
            .Must(ids => ids.Count > 0)
            .WithMessage("At least one supplement member is required.");

        When(
            request => request.SupplementIds is not null,
            () =>
            {
                RuleFor(request => request.SupplementIds)
                    .Must(ids => ids.All(id => id != Guid.Empty))
                    .WithMessage("Supplement identifier is required.");

                RuleFor(request => request.SupplementIds)
                    .Must(ids => ids.Distinct().Count() == ids.Count)
                    .WithMessage("Supplement members must be unique.");
            });
    }
}
