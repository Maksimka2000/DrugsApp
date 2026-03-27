namespace DrugsApp.Domain.TempDrugFeature.Entities;

/// <summary>
/// Lifecycle states for a temporary drug import batch.
/// </summary>
public enum ImportBatchStatus
{
    NotStarted = 0,
    InProgress = 1,
    Cancelled = 2,
    RevertInProgress = 3,
    RevertedSuccessfully = 4,
    ImportedSuccessfully = 5,
    Error = 6
}

