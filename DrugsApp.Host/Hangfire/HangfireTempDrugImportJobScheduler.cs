using DrugsApp.Domain.TempDrugFeature.Interfaces;
using Hangfire;

namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Hangfire-backed scheduler adapter for temp-drug import workflows.
/// </summary>
public sealed class HangfireTempDrugImportJobScheduler : ITempDrugImportJobScheduler
{
    /// <summary>
    /// Execute the Import method 
    /// </summary>
    /// <param name="batchId"></param>
    /// <returns></returns>
    public string Enqueue(Guid batchId)
    {
        return BackgroundJob.Enqueue<TempDrugImportHangfireJob>(
            job => job.ExecuteImportAsync(batchId, CancellationToken.None));
    }

    /// <summary>
    /// Execute Revert method
    /// </summary>
    /// <param name="batchId"></param>
    /// <returns></returns>
    public string EnqueueRevert(Guid batchId)
    {
        return BackgroundJob.Enqueue<TempDrugImportHangfireJob>(
            job => job.ExecuteRevertAsync(batchId, CancellationToken.None));
    }

    /// <summary>
    /// Execute the delete
    /// </summary>
    /// <param name="jobId"></param>
    public void Delete(string? jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return;
        }

        BackgroundJob.Delete(jobId);
    }
}

