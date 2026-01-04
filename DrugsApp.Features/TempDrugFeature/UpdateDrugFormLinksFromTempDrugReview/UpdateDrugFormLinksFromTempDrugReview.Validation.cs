using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.TempDrugFeature.UpdateDrugFormLinksFromTempDrugReview;

public sealed class UpdateDrugFormLinksFromTempDrugReviewRequestValidator
    : AbstractValidator<UpdateDrugFormLinksFromTempDrugReviewRequest>
{
    public UpdateDrugFormLinksFromTempDrugReviewRequestValidator()
    {
        RuleFor(x => x.DrugId)
            .NotEmpty()
            .WithMessage("DrugId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("DrugId must not be empty.");

        RuleFor(x => x.DrugFormId)
            .NotEmpty()
            .WithMessage("DrugFormId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("DrugFormId must not be empty.");

        RuleFor(x => x.CommercialName)
            .NotEmpty()
            .WithMessage("CommercialName is required.")
            .MaximumLength(300)
            .WithMessage("CommercialName must not exceed 300 characters.");

        RuleFor(x => x.TypeOfProduct)
            .NotEmpty()
            .WithMessage("TypeOfProduct is required.")
            .MaximumLength(100)
            .WithMessage("TypeOfProduct must not exceed 100 characters.");

        RuleFor(x => x.ActiveSubstanceIds)
            .NotNull()
            .Must(ids => ids.Length > 0)
            .WithMessage("ActiveSubstanceIds must not be empty.")
            .Must(ids => ids.Distinct().Count() == ids.Length)
            .WithMessage("Duplicate activeSubstanceIds are not allowed.")
            .Must(ids => ids.All(id => id != System.Guid.Empty))
            .WithMessage("ActiveSubstanceIds must not contain empty GUIDs.");

        RuleFor(x => x.ProductIdentifier)
            .NotEmpty()
            .WithMessage("ProductIdentifier is required.")
            .MaximumLength(64)
            .WithMessage("ProductIdentifier must not exceed 64 characters.")
            .Must(value => int.TryParse(value, out int parsed) && parsed > 0)
            .WithMessage("ProductIdentifier must be a positive integer.");

        RuleFor(x => x)
            .Must(x => x.Leaflet is not null || x.EducationalMaterials is not null)
            .WithMessage("At least one of Leaflet or EducationalMaterials must be provided.");

        When(x => x.RouteOfAdministrationIds is not null, () => RuleFor(x => x.RouteOfAdministrationIds!)
                .Must(ids => ids.Length > 0)
                .WithMessage("RouteOfAdministrationIds must not be empty.")
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage("Duplicate routeOfAdministrationIds are not allowed.")
                .Must(ids => ids.All(id => id != System.Guid.Empty))
                .WithMessage("RouteOfAdministrationIds must not contain empty GUIDs."));

        When(x => x.PharmaceuticalFormIds is not null, () => RuleFor(x => x.PharmaceuticalFormIds!)
                .Must(ids => ids.Length > 0)
                .WithMessage("PharmaceuticalFormIds must not be empty.")
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage("Duplicate pharmaceuticalFormIds are not allowed.")
                .Must(ids => ids.All(id => id != System.Guid.Empty))
                .WithMessage("PharmaceuticalFormIds must not contain empty GUIDs."));

        When(x => x.AtcCode is not null, () => RuleFor(x => x.AtcCode)
            .MaximumLength(64)
            .WithMessage("AtcCode must not exceed 64 characters."));

        When(x => x.Dosage is not null, () => RuleFor(x => x.Dosage)
            .MaximumLength(500)
            .WithMessage("Dosage must not exceed 500 characters."));

        When(x => x.Leaflet is not null, () => RuleFor(x => x.Leaflet)
            .MaximumLength(2000)
            .WithMessage("Leaflet must not exceed 2000 characters."));

        When(x => x.EducationalMaterials is not null, () => RuleFor(x => x.EducationalMaterials)
            .MaximumLength(2000)
            .WithMessage("EducationalMaterials must not exceed 2000 characters."));

        When(x => x.Note is not null, () => RuleFor(x => x.Note)
            .MaximumLength(2000)
            .WithMessage("Note must not exceed 2000 characters."));
    }
}

