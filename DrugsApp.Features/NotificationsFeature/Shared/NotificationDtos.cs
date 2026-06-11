using System;
using System.Collections.Generic;

namespace DrugsApp.Features.NotificationsFeature.Shared;

public sealed record UnreadNotificationCountResponse(
    int UnreadCount,
    bool HasActionRequired);

public sealed record NotificationListItemDto(
    Guid Id,
    string TypeCode,
    string PrimaryTagCode,
    string AudienceScopeCode,
    string Title,
    string SummaryText,
    DateTime CreatedAtUtc,
    bool IsRead,
    bool RequiresAction,
    string ActionState,
    string? DeepLinkPath);

public sealed record NotificationListResponse(
    IReadOnlyList<NotificationListItemDto> Items,
    int TotalCount,
    bool HasMore);

public sealed record NotificationDetailResponse(
    Guid Id,
    string TypeCode,
    string PrimaryTagCode,
    string AudienceScopeCode,
    string Title,
    string SummaryText,
    string? BodyText,
    DateTime CreatedAtUtc,
    bool IsRead,
    bool RequiresAction,
    string ActionState,
    string? DeepLinkPath,
    string? RelatedAggregateType,
    Guid? RelatedAggregateId,
    NotificationDetailPayloadDto Detail);

public sealed record NotificationDetailPayloadDto(
    string PayloadType,
    SupervisionRequestDetailBlockDto? SupervisionRequest = null,
    MedicalChangeProposalDetailBlockDto? MedicalChangeProposal = null,
    RelationshipChangeDetailBlockDto? RelationshipChange = null,
    ReminderDetailBlockDto? Reminder = null,
    SecurityDetailBlockDto? Security = null,
    AnnouncementDetailBlockDto? Announcement = null,
    InformationalDetailBlockDto? Informational = null);

public sealed record SupervisionRequestDetailBlockDto(
    Guid RequestId,
    string HealthcareProfessionalSummary,
    string? RequestMessage,
    IReadOnlyList<string> AvailableAccessLevels,
    string? SelectedAccessLevelCode,
    string CurrentRequestState);

public sealed record MedicalChangeProposalDetailBlockDto(
    Guid ProposalId,
    string HealthcareProfessionalSummary,
    string ProposalSummary,
    IReadOnlyList<MedicalChangeProposalSectionBlockDto> Sections,
    string? ProposalNote,
    string CurrentProposalState);

public sealed record MedicalChangeProposalSectionBlockDto(
    string SectionCode,
    string SectionTitle,
    string SummaryText);

public sealed record RelationshipChangeDetailBlockDto(
    string? ActorDisplayName,
    string? ActorRole,
    string? RelationshipSide,
    DateTime? EffectiveAtUtc,
    string? FollowUpPage);

public sealed record ReminderDetailBlockDto(
    string ReminderKind,
    IReadOnlyList<string>? ReviewAreas,
    string? ExpiredItemSummary,
    DateTime? ReminderWindowStartUtc,
    DateTime? ReminderWindowEndUtc,
    DateTime? ExpiredAtUtc);

public sealed record SecurityDetailBlockDto(
    string SecurityPromptType,
    string StatusSummary,
    bool IsAvailableLater);

public sealed record AnnouncementDetailBlockDto(
    string BodyText);

public sealed record InformationalDetailBlockDto(
    string? BodyText);

public sealed record MarkNotificationReadResponse(
    Guid Id,
    bool IsRead,
    DateTime? ReadAtUtc);
