using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using DrugsApp.Features.RegularSupplementsFeature.ApplicationServices;
using DrugsApp.Host.Hangfire;
using ErrorOr;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Host.Hangfire.RegularSupplementsFeature;

/// <summary>
/// Hangfire entrypoint that archives ended regular supplement schedules and projects inbox notifications.
/// </summary>
public sealed class RegularSupplementExpirationHangfireJob(IServiceScopeFactory _serviceScopeFactory)
{
    [Queue(HangfireQueues.RegularIntakes)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IRegularSupplementExpirationApplicationService regularSupplementExpirationApplicationService =
                scope.ServiceProvider.GetRequiredService<IRegularSupplementExpirationApplicationService>();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            ErrorOr<ExpireEndedRegularSupplementsResult> batchResult =
                await regularSupplementExpirationApplicationService.ExpireEndedRowsAsync(cancellationToken);
            if (batchResult.IsError)
            {
                throw new InvalidOperationException(
                    $"Regular supplement expiration failed: {batchResult.FirstError.Description}");
            }

            if (batchResult.Value.ExpiredCount == 0)
            {
                return;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
