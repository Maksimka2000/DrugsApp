namespace DrugsApp.Domain.TempDrugFeature.Interfaces;

/// <summary>
/// Provides read access to Hangfire job status for temp drug imports and reverts.
/// </summary>
public interface ITempDrugImportJobStatusProvider
{
    /// <summary>
    /// Returns the current Hangfire job state for a given job identifier.
    /// </summary>
    /// <param name="hangfireJobId">Background job identifier.</param>
    /// <returns>Current state name or null when unknown/not scheduled.</returns>
    string? GetStatus(string? hangfireJobId);
}
