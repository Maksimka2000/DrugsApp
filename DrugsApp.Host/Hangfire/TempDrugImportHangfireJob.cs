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
        ITempDrugImportApplicationService _tempDrugImportApplicationService = scope.ServiceProvider.GetRequiredService<ITempDrugImportApplicationService>();
        await _tempDrugImportApplicationService.RunImportAsync(batchId, cancellationToken);
    }

    /// <summary>
    /// Executes revert workflow for selected batch.
    /// </summary>
    public async Task ExecuteRevertAsync(Guid batchId, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        ITempDrugRevertApplicationService _tempDrugImportApplicationService = scope.ServiceProvider.GetRequiredService<ITempDrugRevertApplicationService>();
        await _tempDrugImportApplicationService.RunRevertAsync(batchId, cancellationToken);
    }
}

