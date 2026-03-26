using System;
using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.DrugFeature.UpdateDrugForm;

/// <summary>
/// Validator for UpdateDrugForm request payload.
/// </summary>
public sealed class UpdateDrugFormRequestValidator : AbstractValidator<UpdateDrugFormRequest>
{
    public UpdateDrugFormRequestValidator()
    {
        When(x => x.ClearAtcCode != true && x.AtcCode is not null, () =>
        {
            RuleFor(x => x.AtcCode)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("AtcCode must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 32)
                .WithMessage("AtcCode must not exceed 32 characters.");
        });

        When(x => x.ClearDosage != true && x.Dosage is not null, () =>
        {
            RuleFor(x => x.Dosage)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Dosage must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 500)
                .WithMessage("Dosage must not exceed 500 characters.");
        });

        When(x => x.ClearLeaflet != true && x.Leaflet is not null, () =>
        {
            RuleFor(x => x.Leaflet)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Leaflet must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 2000)
                .WithMessage("Leaflet must not exceed 2000 characters.");
        });

        When(x => x.ClearEducationMaterials != true && x.EducationMaterials is not null, () =>
        {
            RuleFor(x => x.EducationMaterials)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("EducationMaterials must not be empty when provided.")
                .Must(value => value!.Trim().Length <= 2000)
                .WithMessage("EducationMaterials must not exceed 2000 characters.");
        });

        When(x => x.RouteOfAdministrationIds is not null, () =>
        {
            RuleFor(x => x.RouteOfAdministrationIds!)
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("RouteOfAdministrationIds must not contain empty Guid values.");

            RuleFor(x => x.RouteOfAdministrationIds!)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate RouteOfAdministrationIds values are not allowed.");

            RuleFor(x => x.RouteOfAdministrationIds!)
                .Must(ids => ids.Count >= 1)
                .WithMessage("Drug form must have at least one route of administration.");
        });

        When(x => x.PharmaceuticalFormIds is not null, () =>
        {
            RuleFor(x => x.PharmaceuticalFormIds!)
                .Must(ids => ids.All(id => id != Guid.Empty))
                .WithMessage("PharmaceuticalFormIds must not contain empty Guid values.");

            RuleFor(x => x.PharmaceuticalFormIds!)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("Duplicate PharmaceuticalFormIds values are not allowed.");

            RuleFor(x => x.PharmaceuticalFormIds!)
                .Must(ids => ids.Count >= 1)
                .WithMessage("Drug form must have at least one pharmaceutical form.");
        });
    }
}

