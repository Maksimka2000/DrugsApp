namespace DrugsApp.Domain.TempDrugFeature.Entities;

/// <summary>
/// Lifecycle states for a temporary drug import batch.
/// </summary>
public enum ImportBatchStatus
{
    /// <summary>
    /// Represents the initial state of a temporary drug import batch.
    /// This status indicates that the import process has not yet begun.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Represents the state where the temporary drug import batch is actively being processed.
    /// This status indicates that the import operation has started but has not yet completed,
    /// and the system is in the process of handling the batch data.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Represents the state where the temporary drug import batch has been cancelled.
    /// This status indicates that the import process was deliberately stopped
    /// and will not proceed further.
    /// </summary>
    Cancelled = 2,

    /// <summary>
    /// Indicates that the revert operation for a temporary drug import batch is currently in progress.
    /// This status is assigned when a batch is undergoing a rollback process to undo previous changes.
    /// </summary>
    RevertInProgress = 3,

    /// <summary>
    /// Indicates that the temporary drug import batch has been reverted successfully.
    /// This status signifies the completion of a revert operation with all associated
    /// records rolled back as intended.
    /// </summary>
    RevertedSuccessfully = 4,

    /// <summary>
    /// Indicates that the import process for the temporary drug batch has completed successfully.
    /// This status confirms that all records have been processed without errors.
    /// </summary>
    ImportedSuccessfully = 5,

    /// <summary>
    /// Indicates that an error occurred during the processing of the temporary drug import batch.
    /// This status represents a failure state where the batch could not complete its operation,
    /// possibly due to validation issues, system errors, or other unexpected conditions.
    /// </summary>
    Error = 6
}

