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
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ErrorOr<ExpireEndedRegularSupplementsResult>> ExpireEndedRowsAsync(
        CancellationToken cancellationToken);
}

/// <summary>
/// Batch result for auto-expire processing.
/// </summary>
public sealed record ExpireEndedRegularSupplementsResult(
    int ExpiredCount,
    int NotificationsCreated);
