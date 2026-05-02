using System;
using FluentValidation;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.DeleteActiveSubstanceGroup;

/// <summary>
/// Validator for delete active substance group request.
/// </summary>
public sealed class DeleteActiveSubstanceGroupRequestValidator : AbstractValidator<DeleteActiveSubstanceGroupRequest>
{
    public DeleteActiveSubstanceGroupRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("Id must be a non-empty GUID.");
    }
}
