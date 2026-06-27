using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.NotificationsFeature.Entities;
using DrugsApp.Domain.NotificationsFeature.Enums;
using DrugsApp.Domain.NotificationsFeature.Interfaces;
using DrugsApp.Domain.NotificationsFeature.Repositories;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.NotificationsFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.NotificationsFeature.ApplicationServices;

/// <summary>
/// Builds rich notification detail payloads based on notification type.
/// </summary>
public interface INotificationDetailHydrationApplicationService
{
    /// <summary>
    /// Hydrates a notification with its detail payload for display.
    /// </summary>
    /// <param name="notification">Notification entity to hydrate.</param>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Hydrated detail payload or errors.</returns>
    Task<ErrorOr<NotificationDetailPayloadDto>> BuildDetailAsync(
        UserNotification notification,
        Guid userId,
        CancellationToken cancellationToken);
}

/// <summary>
/// Default implementation that resolves notification-specific detail payloads.
/// </summary>
public sealed class NotificationDetailHydrationApplicationService(
    IPatientSupervisionNotificationReadService _patientSupervisionNotificationReadService,
    ISystemAnnouncementRepository _systemAnnouncementRepository,
    IUserProfileRepository _userProfileRepository,
    IUserSecurityStatusReadService _userSecurityStatusReadService)
    : INotificationDetailHydrationApplicationService
{
    /// <inheritdoc />
    public async Task<ErrorOr<NotificationDetailPayloadDto>> BuildDetailAsync(
        UserNotification notification,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return notification.TypeCode switch
        {
            UserNotificationType.SupervisionRequest => await BuildSupervisionRequestAsync(notification, userId, cancellationToken),
            UserNotificationType.MedicalChangeProposal => await BuildMedicalChangeProposalAsync(notification, userId, cancellationToken),
            UserNotificationType.SupervisionRemovedByDoctor or UserNotificationType.SupervisionRemovedByPatient
                => await BuildRelationshipChangeAsync(notification, userId, cancellationToken),
            UserNotificationType.SupervisionAccessLevelChanged
                => BuildInformational(notification),
            UserNotificationType.SystemAnnouncement => await BuildSystemAnnouncementAsync(notification, cancellationToken),
            UserNotificationType.WeeklyHealthDataReminder
                or UserNotificationType.SurveyReminder
                or UserNotificationType.HealthcareProviderSurveyReminder
                or UserNotificationType.RegularDrugExpired
                or UserNotificationType.RegularFoodExpired
                or UserNotificationType.RegularSupplementExpired
                => BuildReminder(notification),
            UserNotificationType.EmailVerificationPrompt or UserNotificationType.TwoFactorSuggestion
                => await BuildSecurityAsync(notification, userId, cancellationToken),
            _ => BuildInformational(notification)
        };
    }

    private async Task<ErrorOr<NotificationDetailPayloadDto>> BuildSupervisionRequestAsync(
        UserNotification notification,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (notification.RelatedAggregateId is null)
        {
            return Error.Validation(
                code: "NotificationDetail.MissingRelatedAggregate",
                description: "Supervision request notification is missing related aggregate id.");
        }

        ErrorOr<SupervisionRequestNotificationDetail> detail = await _patientSupervisionNotificationReadService
            .GetRequestDetailAsync(notification.RelatedAggregateId.Value, userId, cancellationToken);
        if (detail.IsError)
        {
            return detail.Errors;
        }

        return new NotificationDetailPayloadDto(
            PayloadType: nameof(UserNotificationType.SupervisionRequest),
            SupervisionRequest: new SupervisionRequestDetailBlockDto(
                detail.Value.RequestId,
                detail.Value.HealthcareProfessionalSummary,
                detail.Value.RequestMessage,
                detail.Value.AvailableAccessLevels,
                detail.Value.SelectedAccessLevelCode,
                detail.Value.CurrentRequestState));
    }

    private async Task<ErrorOr<NotificationDetailPayloadDto>> BuildMedicalChangeProposalAsync(
        UserNotification notification,
        Guid userId,
        CancellationToken cancellationToken)
    {
        if (notification.RelatedAggregateId is null)
        {
            return Error.Validation(
                code: "NotificationDetail.MissingRelatedAggregate",
                description: "Medical change proposal notification is missing related aggregate id.");
        }

        ErrorOr<MedicalChangeProposalNotificationDetail> detail = await _patientSupervisionNotificationReadService
            .GetProposalDetailAsync(notification.RelatedAggregateId.Value, userId, cancellationToken);
        if (detail.IsError)
        {
            return detail.Errors;
        }

        return new NotificationDetailPayloadDto(
            PayloadType: nameof(UserNotificationType.MedicalChangeProposal),
            MedicalChangeProposal: new MedicalChangeProposalDetailBlockDto(
                detail.Value.ProposalId,
                detail.Value.HealthcareProfessionalSummary,
                detail.Value.ProposalSummary,
                detail.Value.Sections
                    .Select(section => new MedicalChangeProposalSectionBlockDto(
                        section.SectionCode,
                        section.SectionTitle,
                        section.SummaryText))
                    .ToList(),
                detail.Value.ProposalNote,
                detail.Value.CurrentProposalState));
    }

    private async Task<ErrorOr<NotificationDetailPayloadDto>> BuildRelationshipChangeAsync(
        UserNotification notification,
        Guid userId,
        CancellationToken cancellationToken)
    {
        string? actorDisplayName = null;
        if (notification.RelatedAggregateId is not null)
        {
            UserProfile? profile = await _userProfileRepository.GetByUserIdAsync(
                notification.RelatedAggregateId.Value,
                cancellationToken);
            if (profile is not null)
            {
                actorDisplayName = $"{profile.FirstName} {profile.LastName}".Trim();
            }
        }

        return new NotificationDetailPayloadDto(
            PayloadType: notification.TypeCode.ToString(),
            RelationshipChange: new RelationshipChangeDetailBlockDto(
                ActorDisplayName: actorDisplayName,
                ActorRole: notification.TypeCode == UserNotificationType.SupervisionRemovedByDoctor
                    ? "HealthcareProvider"
                    : "Client",
                RelationshipSide: notification.RelatedAggregateType,
                EffectiveAtUtc: notification.UpdatedAtUtc ?? notification.CreatedAtUtc,
                FollowUpPage: notification.DeepLinkPath));
    }

    private async Task<ErrorOr<NotificationDetailPayloadDto>> BuildSystemAnnouncementAsync(
        UserNotification notification,
        CancellationToken cancellationToken)
    {
        string bodyText = notification.BodyText ?? notification.SummaryText;
        if (notification.SystemAnnouncementId is not null)
        {
            SystemAnnouncement? announcement = await _systemAnnouncementRepository.GetByIdAsync(
                notification.SystemAnnouncementId.Value,
                cancellationToken);
            if (announcement is not null)
            {
                bodyText = announcement.BodyText;
            }
        }

        return new NotificationDetailPayloadDto(
            PayloadType: nameof(UserNotificationType.SystemAnnouncement),
            Announcement: new AnnouncementDetailBlockDto(bodyText));
    }

    private static ErrorOr<NotificationDetailPayloadDto> BuildReminder(UserNotification notification)
    {
        IReadOnlyList<string>? reviewAreas = notification.TypeCode == UserNotificationType.WeeklyHealthDataReminder
            ? NotificationConstants.WeeklyHealthReviewAreas
            : null;

        return new NotificationDetailPayloadDto(
            PayloadType: notification.TypeCode.ToString(),
            Reminder: new ReminderDetailBlockDto(
                ReminderKind: notification.TypeCode.ToString(),
                ReviewAreas: reviewAreas,
                ExpiredItemSummary: notification.SummaryText,
                ReminderWindowStartUtc: notification.CreatedAtUtc,
                ReminderWindowEndUtc: null,
                ExpiredAtUtc: notification.TypeCode is UserNotificationType.RegularDrugExpired
                    or UserNotificationType.RegularFoodExpired
                    or UserNotificationType.RegularSupplementExpired
                    ? notification.CreatedAtUtc
                    : null));
    }

    private async Task<ErrorOr<NotificationDetailPayloadDto>> BuildSecurityAsync(
        UserNotification notification,
        Guid userId,
        CancellationToken cancellationToken)
    {
        ErrorOr<UserSecurityStatusSnapshot> status = await _userSecurityStatusReadService.GetStatusAsync(
            userId,
            cancellationToken);
        if (status.IsError)
        {
            return status.Errors;
        }

        string statusSummary = notification.TypeCode == UserNotificationType.EmailVerificationPrompt
            ? status.Value.EmailConfirmed ? "Email verified" : "Email verification pending"
            : status.Value.TwoFactorEnabled ? "Two-factor enabled" : "Two-factor not enabled";

        return new NotificationDetailPayloadDto(
            PayloadType: notification.TypeCode.ToString(),
            Security: new SecurityDetailBlockDto(
                SecurityPromptType: notification.TypeCode.ToString(),
                StatusSummary: statusSummary,
                IsAvailableLater: notification.TypeCode == UserNotificationType.TwoFactorSuggestion));
    }

    private static ErrorOr<NotificationDetailPayloadDto> BuildInformational(UserNotification notification)
    {
        return new NotificationDetailPayloadDto(
            PayloadType: "Informational",
            Informational: new InformationalDetailBlockDto(notification.BodyText ?? notification.SummaryText));
    }
}
