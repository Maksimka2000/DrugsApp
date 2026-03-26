using System;
using System.Linq;
using DrugsApp.Domain.DrugFeature.DomainServices;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Features.DrugFeature.Shared;

namespace DrugsApp.Features.DrugFeature.SearchDrugSimilarities;

internal static class SearchDrugSimilaritiesMapping
{
    public static SearchDrugSimilaritiesQuery MapToQuery(this SearchDrugSimilaritiesRequest request) => new(request);

    public static DrugSimilaritySnapshot MapToSnapshot(this SearchDrugSimilaritiesRequest request)
        => new(
            CommercialName: request.CommercialName,
            TypeOfProduct: request.TypeOfProduct,
            ActiveSubstanceIds: request.ActiveSubstanceIds,
            AtcCode: request.AtcCode,
            Dosage: request.Dosage,
            Leaflet: request.Leaflet,
            EducationalMaterials: request.EducationalMaterials,
            RouteOfAdministrationIds: request.RouteOfAdministrationIds,
            PharmaceuticalFormIds: request.PharmaceuticalFormIds);

    public static DrugSimilarityDto MapToDto(
        this DrugSimilarityResult result,
        Drug candidateDrug)
    {
        DrugForm drugForm = candidateDrug.DrugForms.First(form => form.Id == result.DrugFormId);

        return new DrugSimilarityDto(
            MatchScorePercent: result.MatchScorePercent,
            SimilarityClassification: result.SimilarityClassification.MapToApiValue(),
            IsMergeAllowed: result.IsMergeAllowed,
            IsUpdateAllowed: result.IsUpdateAllowed,
            Row: new DrugSimilarityRowDto(
                DrugId: candidateDrug.Id,
                CommercialName: candidateDrug.CommercialName,
                TypeOfProduct: candidateDrug.TypeOfProduct,
                ActiveSubstances: candidateDrug.ActiveSubstances
                    .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
                    .OrderBy(x => x.Name)
                    .ToList(),
                DrugForm: drugForm.MapToDto()));
    }

    private static string MapToApiValue(this DrugSimilarityClassification classification)
        => classification switch
        {
            DrugSimilarityClassification.Duplication => "duplication",
            DrugSimilarityClassification.Update => "update",
            DrugSimilarityClassification.Merge => "merge",
            DrugSimilarityClassification.NameOnly => "name_only",
            _ => "name_only"
        };
}

