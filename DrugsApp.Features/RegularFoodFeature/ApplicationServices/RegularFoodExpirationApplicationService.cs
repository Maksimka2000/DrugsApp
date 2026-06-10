using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Enums;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.RegularFoodFeature.Entities;
using DrugsApp.Domain.RegularFoodFeature.Interfaces;
using DrugsApp.Features.RegularFoodFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.RegularFoodFeature.ApplicationServices;

/// <summary>
/// Application service coordinating regular food auto-expire and notification projection.
/// </summary>
public sealed class RegularFoodExpirationApplicationService(
    IClientRegularFoodRepository _clientRegularFoodRepository,
    INotificationProjectionService _notificationProjectionService) : IRegularFoodExpirationApplicationService
{
    private const int ExpireBatchSize = 50;

    public async Task<ErrorOr<ExpireEndedRegularFoodResult>> ExpireEndedRowsAsync(
        CancellationToken cancellationToken)
    {
        var asOfDate = DateOnly.FromDateTime(DateTime.UtcNow);

        List<ClientRegularFood> candidates = await _clientRegularFoodRepository.ListEndedLiveRowsAsync(
            asOfDate,
            skip: 0,
            take: ExpireBatchSize,
            cancellationToken);

        if (candidates.Count == 0)
        {
            return new ExpireEndedRegularFoodResult(ExpiredCount: 0, NotificationsCreated: 0);
        }

        int expiredCount = 0;
        int notificationsCreated = 0;

        foreach (ClientRegularFood regularFood in candidates)
        {
            ErrorOr<Success> expired = regularFood.ExpireAutomatically(DateTime.UtcNow);
            if (expired.IsError)
            {
                return expired.Errors;
            }

            expiredCount++;

            NotificationProjectionDraft draft = new(
                UserId: regularFood.UserId,
                SystemAnnouncementId: null,
                TypeCode: UserNotificationType.RegularFoodExpired,
                PrimaryTagCode: NotificationTag.MedicalData,
                AudienceScopeCode: NotificationAudienceScope.PatientOnly,
                Title: "Regular food schedule ended",
                SummaryText: RegularFoodMappings.BuildNotificationSummary(regularFood),
                BodyText: null,
                SourceEventKey: $"regular-food-expired:{regularFood.Id}",
                RelatedAggregateType: "client_regular_food",
                RelatedAggregateId: regularFood.Id,
                DeepLinkPath: "/regular-intakes/regular-food/history",
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

        return new ExpireEndedRegularFoodResult(expiredCount, notificationsCreated);
    }
}
