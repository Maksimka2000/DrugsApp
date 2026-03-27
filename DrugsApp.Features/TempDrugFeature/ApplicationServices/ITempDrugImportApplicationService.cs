namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;

/// <summary>
/// Orchestrates background import execution workflow.
/// </summary>
public interface ITempDrugImportApplicationService
{
    /// <summary>
    /// Executes import processing for a selected batch id.
    /// </summary>
    Task RunImportAsync(Guid batchId, CancellationToken cancellationToken);
}

