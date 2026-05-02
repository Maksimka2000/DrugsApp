using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;

/// <summary>
/// Validator for creating active substance groups.
/// </summary>
public sealed class AddActiveSubstanceGroupRequestValidator : AbstractValidator<AddActiveSubstanceGroupRequest>
{
    public AddActiveSubstanceGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ActiveSubstanceIds)
            .NotEmpty()
            .WithMessage("At least two active substances are required.")
            .Must(HaveAtLeastTwoItems)
            .WithMessage("At least two active substances are required.")
            .Must(NotContainDuplicates)
            .WithMessage("Duplicate active substance identifiers are not allowed.")
            .Must(NotContainEmptyIds)
            .WithMessage("Active substance identifiers must be valid GUID values.");
    }

    private static bool HaveAtLeastTwoItems(IReadOnlyCollection<Guid> ids) => ids.Count >= 2;

    private static bool NotContainDuplicates(IReadOnlyCollection<Guid> ids) =>
        ids.Distinct().Count() == ids.Count;

    private static bool NotContainEmptyIds(IReadOnlyCollection<Guid> ids) =>
        ids.All(id => id != Guid.Empty);
}
