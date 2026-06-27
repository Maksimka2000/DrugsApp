using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Features.RegularSupplementsFeature.ApplicationServices;

/// <summary>
/// Orchestrates regular supplement auto-expire workflows for background jobs.
/// </summary>
public interface IRegularSupplementExpirationApplicationService
{
    /// <summary>
    /// Expires ended regular supplement rows and projects notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Batch result with counts.</returns>
    Task<ErrorOr<ExpireEndedRegularSupplementsResult>> ExpireEndedRowsAsync(
        CancellationToken cancellationToken);
}

/// <summary>
/// Batch result for auto-expire processing.
/// </summary>
public sealed record ExpireEndedRegularSupplementsResult(
    int ExpiredCount,
    int NotificationsCreated);
