namespace DrugsApp.Domain.DrugFeature.Entities;

/// <summary>
/// Enumerates supported sort fields for drug search listings.
/// </summary>
public enum DrugSortField
{
    /// <summary>
    /// Sort by the commercial name (default).
    /// </summary>
    CommercialName = 0,

    /// <summary>
    /// Sort by the creation timestamp.
    /// </summary>
    CreatedAt = 1,

    /// <summary>
    /// Sort by the last update timestamp.
    /// </summary>
    UpdatedAt = 2
}

