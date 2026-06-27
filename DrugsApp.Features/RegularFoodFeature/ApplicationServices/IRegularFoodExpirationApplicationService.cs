using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Features.RegularFoodFeature.ApplicationServices;

/// <summary>
/// Application service contract for regular food auto-expire workflows.
/// </summary>
public interface IRegularFoodExpirationApplicationService
{
    /// <summary>
    /// Expires ended regular food schedules and emits notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Batch result with expired and notification counts.</returns>
    Task<ErrorOr<ExpireEndedRegularFoodResult>> ExpireEndedRowsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Internal result for a single auto-expire batch.
/// </summary>
public sealed record ExpireEndedRegularFoodResult(int ExpiredCount, int NotificationsCreated);
