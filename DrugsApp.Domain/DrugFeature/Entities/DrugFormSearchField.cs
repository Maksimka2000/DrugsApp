namespace DrugsApp.Domain.DrugFeature.Entities;

/// <summary>
/// Describes the prioritized field used when running a drug form search query.
/// </summary>
public enum DrugFormSearchField
{
    /// <summary>
    /// No field-based filtering (used when no search query is provided).
    /// </summary>
    None = 0,

    /// <summary>
    /// Search by the parent drug commercial name.
    /// </summary>
    CommercialName = 1,

    /// <summary>
    /// Search by any linked active substance name on the parent drug.
    /// </summary>
    ActiveSubstance = 2,

    /// <summary>
    /// Search by veterinary drug commercial name in the VetDrug catalog.
    /// </summary>
    VeterinaryCommercialName = 3
}
