using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Enums;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.RegularSupplementsFeature.Entities;
using DrugsApp.Domain.RegularSupplementsFeature.Enums;
using DrugsApp.Domain.RegularSupplementsFeature.Repositories;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.RegularSupplementsFeature.ApplicationServices;

/// <summary>
/// Application service coordinating regular supplement auto-expire and notification projection.
/// </summary>
public sealed class RegularSupplementExpirationApplicationService(
    IClientRegularSupplementRepository _clientRegularSupplementRepository,
    INotificationProjectionService _notificationProjectionService) : IRegularSupplementExpirationApplicationService
{
    private const int ExpireBatchSize = 50;

    /// <summary>
    /// Expires ended supplement schedules and emits corresponding notifications.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Batch result with expired and notification counts.</returns>
    public async Task<ErrorOr<ExpireEndedRegularSupplementsResult>> ExpireEndedRowsAsync(
        CancellationToken cancellationToken)
    {
        var asOfDate = DateOnly.FromDateTime(DateTime.UtcNow);

        List<ClientRegularSupplement> candidates = await _clientRegularSupplementRepository.FindEndedLiveRowsAsync(
            asOfDate,
            ExpireBatchSize,
            cancellationToken);

        if (candidates.Count == 0)
        {
            return new ExpireEndedRegularSupplementsResult(ExpiredCount: 0, NotificationsCreated: 0);
        }

        int expiredCount = 0;
        int notificationsCreated = 0;

        foreach (ClientRegularSupplement regularSupplement in candidates)
        {
            ErrorOr<Success> moved = regularSupplement.MoveToHistory(
                RegularSupplementHistoryReason.AutoExpire,
                DateTime.UtcNow);
            if (moved.IsError)
            {
                return moved.Errors;
            }

            expiredCount++;

            NotificationProjectionDraft draft = new(
                UserId: regularSupplement.UserId,
                SystemAnnouncementId: null,
                TypeCode: UserNotificationType.RegularSupplementExpired,
                PrimaryTagCode: NotificationTag.MedicalData,
                AudienceScopeCode: NotificationAudienceScope.PatientOnly,
                Title: "Regular supplement schedule ended",
                SummaryText: RegularSupplementMappings.BuildNotificationSummary(regularSupplement),
                BodyText: null,
                SourceEventKey: $"regular-supplement-expired:{regularSupplement.Id}",
                RelatedAggregateType: "client_regular_supplement",
                RelatedAggregateId: regularSupplement.Id,
                DeepLinkPath: "/regular-intakes/regular-supplements/history",
                RequiresAction: false,
                ActionState: NotificationActionState.Informational);

            ErrorOr<Success> projectionResult = await _notificationProjectionService.ProjectAsync(
                draft,
                cancellationToken);
            if (projectionResult.IsError)
            {
                return projectionResult.Errors;
            }

            notificationsCreated++;
        }

        return new ExpireEndedRegularSupplementsResult(expiredCount, notificationsCreated);
    }
}
