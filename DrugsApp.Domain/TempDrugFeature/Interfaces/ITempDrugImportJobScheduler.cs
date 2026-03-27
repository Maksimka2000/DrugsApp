namespace DrugsApp.Domain.TempDrugFeature.Interfaces;

/// <summary>
/// Schedules import/revert batch jobs in background processing infrastructure.
/// </summary>
public interface ITempDrugImportJobScheduler
{
    /// <summary>
    /// Enqueues import execution job for batch id.
    /// </summary>
    string Enqueue(Guid batchId);

    /// <summary>
    /// Enqueues revert execution job for batch id.
    /// </summary>
    string EnqueueRevert(Guid batchId);

    /// <summary>
    /// Deletes a previously scheduled job by infrastructure identifier.
    /// </summary>
    void Delete(string? jobId);
}

