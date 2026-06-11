using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.RegularDrugsFeature.Shared;
using DrugsApp.Domain.VetDrugFeature.Entities;

namespace DrugsApp.Features.DrugFeature.Shared;

/// <summary>
/// Mapping helpers for drug form search candidate rows.
/// </summary>
internal static class SearchDrugFormMappings
{
    private const string HumanProductCategory = "human";
    private const string VeterinaryProductCategory = "veterinary";
    private const string VeterinaryProductLabel = "Weterynaryjny";

    public static SearchDrugFormDto MapCatalogFormToDto(DrugForm drugForm)
    {
        Drug drug = drugForm.Drug;
        IReadOnlyList<LookupDto> activeSubstances = drug.ActiveSubstances
            .OrderBy(activeSubstance => activeSubstance.ActiveSubstanceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(activeSubstance => activeSubstance.Id)
            .Select(activeSubstance => new LookupDto(activeSubstance.Id, activeSubstance.ActiveSubstanceName))
            .ToList();

        IReadOnlyList<LookupDto> pharmaceuticalForms = drugForm.PharmaceuticalForms
            .OrderBy(form => form.FormulationName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(form => form.Id)
            .Select(form => new LookupDto(form.Id, form.FormulationName))
            .ToList();

        bool isVeterinary = IsVeterinaryCatalogDrug(drug);

        return new SearchDrugFormDto(
            DrugId: drug.Id,
            DrugFormId: drugForm.Id,
            CommercialName: drug.CommercialName,
            TypeOfProduct: isVeterinary ? VeterinaryProductCategory : HumanProductCategory,
            TypeOfDrug: drug.TypeOfDrug,
            ActiveSubstances: activeSubstances,
            Dosage: drugForm.Dosage,
            IsOral: drugForm.IsOral,
            PharmaceuticalForms: pharmaceuticalForms,
            DisplayLine: BuildCatalogDisplayLine(drug, drugForm, activeSubstances, pharmaceuticalForms),
            IsSelectable: RegularDrugEligibility.IsEligibleDrug(drug));
    }

    public static SearchDrugFormDto MapVetDrugToDto(VetDrug vetDrug) =>
        new(
            DrugId: vetDrug.Id,
            DrugFormId: null,
            CommercialName: vetDrug.CommercialName,
            TypeOfProduct: VeterinaryProductCategory,
            TypeOfDrug: DrugType.Standard,
            ActiveSubstances: Array.Empty<LookupDto>(),
            Dosage: null,
            IsOral: false,
            PharmaceuticalForms: Array.Empty<LookupDto>(),
            DisplayLine: BuildVetDisplayLine(vetDrug.CommercialName),
            IsSelectable: false);

    private static string BuildCatalogDisplayLine(
        Drug drug,
        DrugForm drugForm,
        IReadOnlyCollection<LookupDto> activeSubstances,
        IReadOnlyCollection<LookupDto> pharmaceuticalForms)
    {
        List<string> parts = new();

        string commercialNameSegment = drug.CommercialName;
        if (activeSubstances.Count > 0)
        {
            string activeSubstanceNames = string.Join(
                ", ",
                activeSubstances.Select(activeSubstance => activeSubstance.Name));
            commercialNameSegment = $"{drug.CommercialName} ({activeSubstanceNames})";
        }

        parts.Add(commercialNameSegment);

        string pharmaceuticalFormSegment = string.Join(
            ", ",
            pharmaceuticalForms.Select(form => form.Name));

        if (!string.IsNullOrWhiteSpace(pharmaceuticalFormSegment))
        {
            parts.Add(pharmaceuticalFormSegment);
        }

        if (!string.IsNullOrWhiteSpace(drugForm.Dosage))
        {
            parts.Add(drugForm.Dosage);
        }

        if (drug.TypeOfDrug is DrugType.Special or DrugType.Ophthalmic)
        {
            parts.Add(drug.TypeOfDrug.ToString());
        }

        if (IsVeterinaryCatalogDrug(drug))
        {
            parts.Add(VeterinaryProductLabel);
        }

        return string.Join(" | ", parts);
    }

    private static string BuildVetDisplayLine(string commercialName) =>
        $"{commercialName} | {VeterinaryProductLabel}";

    private static bool IsVeterinaryCatalogDrug(Drug drug) =>
        drug.TypeOfProduct.Equals(VeterinaryProductLabel, StringComparison.OrdinalIgnoreCase);
}
