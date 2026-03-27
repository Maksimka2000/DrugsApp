namespace DrugsApp.Features.TempDrugFeature.ApplicationServices;

/// <summary>
/// Orchestrates background revert execution workflow.
/// </summary>
public interface ITempDrugRevertApplicationService
{
    /// <summary>
    /// Executes revert processing for selected batch id.
    /// </summary>
    Task RunRevertAsync(Guid batchId, CancellationToken cancellationToken);
}

