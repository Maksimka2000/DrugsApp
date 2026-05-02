using System.Linq;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Retrieves Hangfire job status for temp drug import/revert jobs.
/// </summary>
public sealed class HangfireTempDrugImportJobStatusProvider : ITempDrugImportJobStatusProvider
{
    /// <summary>
    /// Get status of the hangfire job
    /// </summary>
    /// <param name="hangfireJobId"></param>
    /// <returns></returns>
    public string? GetStatus(string? hangfireJobId)
    {
        if (string.IsNullOrWhiteSpace(hangfireJobId))
        {
            return null;
        }

        JobDetailsDto? details = JobStorage.Current.GetMonitoringApi().JobDetails(hangfireJobId);
        if (details is null)
        {
            return null;
        }

        return details.History?.FirstOrDefault()?.StateName;
    }
}
