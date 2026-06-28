using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using DrugsApp.Features.RegularFoodFeature.ApplicationServices;
using DrugsApp.Host.Hangfire;
using ErrorOr;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Host.Hangfire.RegularFoodFeature;

/// <summary>
/// Hangfire entrypoint that archives ended regular food schedules and projects inbox notifications.
/// </summary>
public sealed class RegularFoodExpirationHangfireJob(IServiceScopeFactory _serviceScopeFactory)
{
    /// <summary>
    /// Expires ended regular food rows in batches and projects notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel processing.</param>
    [Queue(HangfireQueues.RegularIntakes)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IRegularFoodExpirationApplicationService regularFoodExpirationApplicationService =
                scope.ServiceProvider.GetRequiredService<IRegularFoodExpirationApplicationService>();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            ErrorOr<ExpireEndedRegularFoodResult> batchResult =
                await regularFoodExpirationApplicationService.ExpireEndedRowsAsync(cancellationToken);
            if (batchResult.IsError)
            {
                throw new InvalidOperationException(
                    $"Regular food expiration failed: {batchResult.FirstError.Description}");
            }

            if (batchResult.Value.ExpiredCount == 0)
            {
                return;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
