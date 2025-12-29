namespace DrugsApp.Domain.TempDrugFeature.Entities;

/// <summary>
/// Represents the review lifecycle state of an imported temporary drug.
/// </summary>
public enum TempDrugStatus
{
    /// <summary>
    /// Item is waiting for initial review.
    /// </summary>
    PendingReview = 0,

    /// <summary>
    /// Item was deferred for later review.
    /// </summary>
    ReviewLater = 1,

    /// <summary>
    /// Item was reviewed and resolved without adding to the database.
    /// </summary>
    ReviewedResolved = 2,

    /// <summary>
    /// Item was reviewed and merged into the canonical database.
    /// </summary>
    ReviewedAdded = 3
}


