namespace DrugsApp.Domain.TempDrugFeature.Entities;

/// <summary>
/// Defines stable sort fields available for temporary drug search queries.
/// </summary>
public enum TempDrugSortField
{
    /// <summary>
    /// Sort by creation timestamp.
    /// </summary>
    CreatedAtUtc = 0,

    /// <summary>
    /// Sort by last update timestamp.
    /// </summary>
    UpdatedAtUtc = 1,

    /// <summary>
    /// Sort by product identifier.
    /// </summary>
    ProductIdentifier = 2,

    /// <summary>
    /// Sort by commercial name.
    /// </summary>
    CommercialName = 3
}


