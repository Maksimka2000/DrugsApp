using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.Common;
using DrugsApp.Features.RegularDrugsFeature.ApplicationServices;
using DrugsApp.Host.Hangfire;
using ErrorOr;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Host.Hangfire.RegularDrugsFeature;

/// <summary>
/// Hangfire entrypoint that archives ended regular drug schedules and projects inbox notifications.
/// </summary>
public sealed class RegularDrugExpirationHangfireJob(IServiceScopeFactory _serviceScopeFactory)
{
    [Queue(HangfireQueues.RegularIntakes)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            IRegularDrugsApplicationService regularDrugsApplicationService =
                scope.ServiceProvider.GetRequiredService<IRegularDrugsApplicationService>();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            ErrorOr<ExpireEndedRegularDrugsResult> batchResult =
                await regularDrugsApplicationService.ExpireEndedRowsAsync(cancellationToken);
            if (batchResult.IsError)
            {
                throw new InvalidOperationException(
                    $"Regular drug expiration failed: {batchResult.FirstError.Description}");
            }

            if (batchResult.Value.ExpiredCount == 0)
            {
                return;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
