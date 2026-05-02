using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;

/// <summary>
/// Validator for updating active substance groups.
/// </summary>
public sealed class UpdateActiveSubstanceGroupRequestValidator : AbstractValidator<UpdateActiveSubstanceGroupRequest>
{
    public UpdateActiveSubstanceGroupRequestValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.Name), () => RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters."));

        When(x => x.ActiveSubstanceIds is not null, () => 
        {
            RuleFor(x => x.ActiveSubstanceIds!)
                .NotEmpty()
                .WithMessage("Active substance list must contain at least two items.")
                .Must(HaveAtLeastTwoItems)
                .WithMessage("Active substance list must contain at least two items.")
                .Must(NotContainDuplicates)
                .WithMessage("Duplicate active substance identifiers are not allowed.")
                .Must(NotContainEmptyIds)
                .WithMessage("Active substance identifiers must be valid GUID values.");
        });
    }

    private static bool HaveAtLeastTwoItems(IReadOnlyCollection<Guid> ids) => ids.Count >= 2;

    private static bool NotContainDuplicates(IReadOnlyCollection<Guid> ids) =>
        ids.Distinct().Count() == ids.Count;

    private static bool NotContainEmptyIds(IReadOnlyCollection<Guid> ids) =>
        ids.All(id => id != Guid.Empty);
}
