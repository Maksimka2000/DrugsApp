using DrugsApp.Features.TempDrugFeature.ApplicationServices;

namespace DrugsApp.Host.Hangfire;

/// <summary>
/// Hangfire job entrypoint for import and revert execution.
/// </summary>
public sealed class TempDrugImportHangfireJob(IServiceScopeFactory _serviceScopeFactory)
{
    
    /// <summary>
    /// Executes import workflow for selected batch.
    /// </summary>
    public async Task ExecuteImportAsync(Guid batchId, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        ITempDrugImportApplicationService importApplicationService = scope.ServiceProvider.GetRequiredService<ITempDrugImportApplicationService>();
        await importApplicationService.RunImportAsync(batchId, cancellationToken);
    }

    /// <summary>
    /// Executes revert workflow for selected batch.
    /// </summary>
    public async Task ExecuteRevertAsync(Guid batchId, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        ITempDrugRevertApplicationService revertApplicationService = scope.ServiceProvider.GetRequiredService<ITempDrugRevertApplicationService>();
        await revertApplicationService.RunRevertAsync(batchId, cancellationToken);
    }
}

