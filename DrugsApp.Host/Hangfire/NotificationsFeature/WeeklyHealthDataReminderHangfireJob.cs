using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.NotificationsFeature;
using DrugsApp.Domain.NotificationsFeature.Enums;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Host.Hangfire;
using DrugsApp.Infrastructure.Persistence.Identity;
using ErrorOr;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DrugsApp.Host.Hangfire.NotificationsFeature;

/// <summary>
/// Hangfire entrypoint that projects weekly health-data reminder inbox rows.
/// </summary>
public sealed class WeeklyHealthDataReminderHangfireJob(IServiceScopeFactory _serviceScopeFactory)
{
    [Queue(HangfireQueues.Notifications)]
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        int isoWeek = ISOWeek.GetWeekOfYear(DateTime.UtcNow);
        if (isoWeek % 2 != 0)
        {
            return;
        }

        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        INotificationProjectionService notificationProjectionService =
            scope.ServiceProvider.GetRequiredService<INotificationProjectionService>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        IList<ApplicationUser> clientUsers = await userManager.GetUsersInRoleAsync(AuthRoles.Client);
        DateTime weekStartUtc = GetWeekStartUtc(DateTime.UtcNow);
        string sourceEventKey = $"weekly-health:{weekStartUtc:yyyyMMdd}";

        foreach (ApplicationUser user in clientUsers)
        {
            NotificationProjectionDraft draft = new(
                UserId: user.Id,
                SystemAnnouncementId: null,
                TypeCode: UserNotificationType.WeeklyHealthDataReminder,
                PrimaryTagCode: NotificationTag.MedicalData,
                AudienceScopeCode: NotificationAudienceScope.PatientOnly,
                Title: "Review your health data",
                SummaryText: "Keep your allergies, conditions, and regular intake records up to date this week.",
                BodyText: null,
                SourceEventKey: sourceEventKey,
                RelatedAggregateType: "account_health_data",
                RelatedAggregateId: null,
                DeepLinkPath: "/health-data",
                RequiresAction: false);

            ErrorOr<Success> projectionResult = await notificationProjectionService.ProjectAsync(
                draft,
                cancellationToken);
            if (projectionResult.IsError)
            {
                throw new InvalidOperationException(
                    $"Weekly health reminder projection failed for user {user.Id}: {projectionResult.FirstError.Description}");
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static DateTime GetWeekStartUtc(DateTime utcNow)
    {
        int daysSinceMonday = ((int)utcNow.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return utcNow.Date.AddDays(-daysSinceMonday);
    }
}
