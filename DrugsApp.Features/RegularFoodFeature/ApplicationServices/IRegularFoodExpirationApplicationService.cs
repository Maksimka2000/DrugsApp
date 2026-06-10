using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace DrugsApp.Features.RegularFoodFeature.ApplicationServices;

/// <summary>
/// Application service contract for regular food auto-expire workflows.
/// </summary>
public interface IRegularFoodExpirationApplicationService
{
    Task<ErrorOr<ExpireEndedRegularFoodResult>> ExpireEndedRowsAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Internal result for a single auto-expire batch.
/// </summary>
public sealed record ExpireEndedRegularFoodResult(int ExpiredCount, int NotificationsCreated);
