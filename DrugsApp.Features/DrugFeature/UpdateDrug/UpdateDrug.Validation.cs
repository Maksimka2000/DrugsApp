using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.DrugFeature.UpdateDrug;

/// <summary>
/// Validator for UpdateDrug request payload.
/// </summary>
public sealed class UpdateDrugRequestValidator : AbstractValidator<UpdateDrugRequest>
{
    public UpdateDrugRequestValidator()
    {
        When(x => x.CommercialName is not null, () =>
        {
            RuleFor(x => x.CommercialName)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("CommercialName must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 300)
                .WithMessage("CommercialName must not exceed 300 characters.");
        });

        When(x => x.NewActiveSubstanceIds is not null, () =>
        {
            RuleFor(x => x.NewActiveSubstanceIds!)
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("NewActiveSubstanceIds must not contain empty Guid values.");

            RuleFor(x => x.NewActiveSubstanceIds!)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate active substance ids are not allowed.");

            RuleFor(x => x.NewActiveSubstanceIds!)
                .Must(ids => ids.Count >= 1)
                .WithMessage("Drug must have at least one active substance.");
        });

        When(x => x.TypeOfDrug is not null, () =>
        {
            RuleFor(x => x.TypeOfDrug)
                .IsInEnum()
                .WithMessage("Invalid TypeOfDrug value.");
        });
    }
}

