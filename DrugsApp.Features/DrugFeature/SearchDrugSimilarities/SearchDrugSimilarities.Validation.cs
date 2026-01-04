using System.Linq;
using FluentValidation;

namespace DrugsApp.Features.DrugFeature.SearchDrugSimilarities;

public sealed class SearchDrugSimilaritiesRequestValidator : AbstractValidator<SearchDrugSimilaritiesRequest>
{
    public SearchDrugSimilaritiesRequestValidator()
    {
        RuleFor(x => x.CommercialName)
            .NotEmpty()
            .WithMessage("CommercialName is required.")
            .MaximumLength(300)
            .WithMessage("CommercialName must not exceed 300 characters.");

        RuleFor(x => x.TypeOfProduct)
            .NotEmpty()
            .MaximumLength(100);

        When(x => x.ActiveSubstanceIds is not null, () => RuleFor(x => x.ActiveSubstanceIds!)
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage("Duplicate activeSubstanceIds are not allowed.")
                .Must(ids => ids.All(id => id != System.Guid.Empty))
                .WithMessage("ActiveSubstanceIds must not contain empty GUIDs."));

        When(x => x.RouteOfAdministrationIds is not null, () => RuleFor(x => x.RouteOfAdministrationIds!)
                .Must(ids => ids.Distinct().Count() == ids.Length)
                .WithMessage("Duplicate routeOfAdministrationIds are not allowed.")
                .Must(ids => ids.All(id => id != System.Guid.Empty))
                .WithMessage("RouteOfAdministrationIds must not contain empty GUIDs."));

        When(x => x.PharmaceuticalFormIds is not null, () => RuleFor(x => x.PharmaceuticalFormIds!)
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
    }
}

