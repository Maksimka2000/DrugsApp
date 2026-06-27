using System;
using System.Collections.Generic;

namespace DrugsApp.Features.NotificationsFeature.Shared;

/// <summary>
/// Response carrying unread notification count.
/// </summary>
/// <param name="UnreadCount">Number of unread notifications.</param>
/// <param name="HasActionRequired">Indicates whether any notification requires action.</param>
public sealed record UnreadNotificationCountResponse(
    int UnreadCount,
    bool HasActionRequired);

/// <summary>
/// Notification list item DTO.
/// </summary>
/// <param name="Id">Notification identifier.</param>
/// <param name="TypeCode">Notification type code.</param>
/// <param name="PrimaryTagCode">Primary tag classification.</param>
/// <param name="AudienceScopeCode">Audience scope code.</param>
/// <param name="Title">Notification title.</param>
/// <param name="SummaryText">Notification summary.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="IsRead">Indicates if notification is read.</param>
/// <param name="RequiresAction">Indicates if action is required.</param>
/// <param name="ActionState">Current action state.</param>
/// <param name="DeepLinkPath">Optional deep link path.</param>
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

/// <summary>
/// Notification list response with paging metadata.
/// </summary>
/// <param name="Items">Items for the page.</param>
/// <param name="TotalCount">Total notification count.</param>
/// <param name="HasMore">Indicates if more pages exist.</param>
public sealed record NotificationListResponse(
    IReadOnlyList<NotificationListItemDto> Items,
    int TotalCount,
    bool HasMore);

/// <summary>
/// Detailed notification response.
/// </summary>
/// <param name="Id">Notification identifier.</param>
/// <param name="TypeCode">Notification type code.</param>
/// <param name="PrimaryTagCode">Primary tag classification.</param>
/// <param name="AudienceScopeCode">Audience scope code.</param>
/// <param name="Title">Title of the notification.</param>
/// <param name="SummaryText">Summary text.</param>
/// <param name="BodyText">Optional body text.</param>
/// <param name="CreatedAtUtc">Creation timestamp.</param>
/// <param name="IsRead">Whether the notification is read.</param>
/// <param name="RequiresAction">Whether action is required.</param>
/// <param name="ActionState">Action state string.</param>
/// <param name="DeepLinkPath">Optional deep link path.</param>
/// <param name="RelatedAggregateType">Related aggregate type.</param>
/// <param name="RelatedAggregateId">Related aggregate id.</param>
/// <param name="Detail">Hydrated detail payload.</param>
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

/// <summary>
/// Polymorphic notification detail payload.
/// </summary>
/// <param name="PayloadType">Type discriminator.</param>
/// <param name="SupervisionRequest">Supervision request payload.</param>
/// <param name="MedicalChangeProposal">Medical change proposal payload.</param>
/// <param name="RelationshipChange">Relationship change payload.</param>
/// <param name="Reminder">Reminder payload.</param>
/// <param name="Security">Security payload.</param>
/// <param name="Announcement">Announcement payload.</param>
/// <param name="Informational">Informational payload.</param>
public sealed record NotificationDetailPayloadDto(
    string PayloadType,
    SupervisionRequestDetailBlockDto? SupervisionRequest = null,
    MedicalChangeProposalDetailBlockDto? MedicalChangeProposal = null,
    RelationshipChangeDetailBlockDto? RelationshipChange = null,
    ReminderDetailBlockDto? Reminder = null,
    SecurityDetailBlockDto? Security = null,
    AnnouncementDetailBlockDto? Announcement = null,
    InformationalDetailBlockDto? Informational = null);

/// <summary>
/// Detail block for supervision requests.
/// </summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="HealthcareProfessionalSummary">Healthcare professional summary.</param>
/// <param name="RequestMessage">Optional request message.</param>
/// <param name="AvailableAccessLevels">Available access levels.</param>
/// <param name="SelectedAccessLevelCode">Selected access level code.</param>
/// <param name="CurrentRequestState">Current request state.</param>
public sealed record SupervisionRequestDetailBlockDto(
    Guid RequestId,
    string HealthcareProfessionalSummary,
    string? RequestMessage,
    IReadOnlyList<string> AvailableAccessLevels,
    string? SelectedAccessLevelCode,
    string CurrentRequestState);

/// <summary>
/// Detail block for medical change proposals.
/// </summary>
/// <param name="ProposalId">Proposal identifier.</param>
/// <param name="HealthcareProfessionalSummary">Healthcare professional summary.</param>
/// <param name="ProposalSummary">Proposal summary text.</param>
/// <param name="Sections">Proposal sections.</param>
/// <param name="ProposalNote">Optional proposal note.</param>
/// <param name="CurrentProposalState">Current proposal state.</param>
public sealed record MedicalChangeProposalDetailBlockDto(
    Guid ProposalId,
    string HealthcareProfessionalSummary,
    string ProposalSummary,
    IReadOnlyList<MedicalChangeProposalSectionBlockDto> Sections,
    string? ProposalNote,
    string CurrentProposalState);

/// <summary>
/// Section detail within a medical change proposal.
/// </summary>
/// <param name="SectionCode">Section code.</param>
/// <param name="SectionTitle">Section title.</param>
/// <param name="SummaryText">Summary text.</param>
public sealed record MedicalChangeProposalSectionBlockDto(
    string SectionCode,
    string SectionTitle,
    string SummaryText);

/// <summary>
/// Relationship change notification detail.
/// </summary>
/// <param name="ActorDisplayName">Display name of the actor.</param>
/// <param name="ActorRole">Role of the actor.</param>
/// <param name="RelationshipSide">Which side of the relationship changed.</param>
/// <param name="EffectiveAtUtc">Effective timestamp.</param>
/// <param name="FollowUpPage">Optional follow-up link.</param>
public sealed record RelationshipChangeDetailBlockDto(
    string? ActorDisplayName,
    string? ActorRole,
    string? RelationshipSide,
    DateTime? EffectiveAtUtc,
    string? FollowUpPage);

/// <summary>
/// Reminder notification detail block.
/// </summary>
/// <param name="ReminderKind">Kind of reminder.</param>
/// <param name="ReviewAreas">Optional review areas.</param>
/// <param name="ExpiredItemSummary">Optional expired item summary.</param>
/// <param name="ReminderWindowStartUtc">Start of reminder window.</param>
/// <param name="ReminderWindowEndUtc">End of reminder window.</param>
/// <param name="ExpiredAtUtc">Optional expiration timestamp.</param>
public sealed record ReminderDetailBlockDto(
    string ReminderKind,
    IReadOnlyList<string>? ReviewAreas,
    string? ExpiredItemSummary,
    DateTime? ReminderWindowStartUtc,
    DateTime? ReminderWindowEndUtc,
    DateTime? ExpiredAtUtc);

/// <summary>
/// Security notification detail block.
/// </summary>
/// <param name="SecurityPromptType">Prompt type.</param>
/// <param name="StatusSummary">Status summary.</param>
/// <param name="IsAvailableLater">Whether the prompt can be deferred.</param>
public sealed record SecurityDetailBlockDto(
    string SecurityPromptType,
    string StatusSummary,
    bool IsAvailableLater);

/// <summary>
/// System announcement detail block.
/// </summary>
/// <param name="BodyText">Body text of the announcement.</param>
public sealed record AnnouncementDetailBlockDto(
    string BodyText);

/// <summary>
/// Informational notification detail block.
/// </summary>
/// <param name="BodyText">Optional body text.</param>
public sealed record InformationalDetailBlockDto(
    string? BodyText);

/// <summary>
/// Response for marking a notification as read.
/// </summary>
/// <param name="Id">Notification identifier.</param>
/// <param name="IsRead">Updated read state.</param>
/// <param name="ReadAtUtc">Timestamp when read.</param>
public sealed record MarkNotificationReadResponse(
    Guid Id,
    bool IsRead,
    DateTime? ReadAtUtc);
