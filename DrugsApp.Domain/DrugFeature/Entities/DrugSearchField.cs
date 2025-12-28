namespace DrugsApp.Domain.DrugFeature.Entities;

/// <summary>
/// Describes the prioritized field used when running a drug search query.
/// </summary>
public enum DrugSearchField
{
    /// <summary>
    /// No field-based filtering (used when no search query is provided).
    /// </summary>
    None = 0,

    /// <summary>
    /// Search by the drug commercial name.
    /// </summary>
    CommercialName = 1,

    /// <summary>
    /// Search by any linked active substance name.
    /// </summary>
    ActiveSubstance = 2,

    /// <summary>
    /// Search by drug form ATC code.
    /// </summary>
    AtcCode = 3,

    /// <summary>
    /// Search by linked route of administration name.
    /// </summary>
    RouteOfAdministration = 4,

    /// <summary>
    /// Search by linked pharmaceutical form name.
    /// </summary>
    PharmaceuticalForm = 5,

    /// <summary>
    /// Search by exact product identifier.
    /// </summary>
    ProductIdentifier = 6
}

