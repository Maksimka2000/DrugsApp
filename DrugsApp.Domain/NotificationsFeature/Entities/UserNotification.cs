using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.NotificationsFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.NotificationsFeature.Entities;

/// <summary>
/// Per-user inbox projection row with denormalized list fields and source pointers.
/// </summary>
public sealed class UserNotification : BaseEntity
{
    // Attributes

    /// <summary>
    /// Authenticated owner identifier from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Optional link to a system announcement fan-out source.
    /// </summary>
    public Guid? SystemAnnouncementId { get; private set; }

    /// <summary>
    /// Notification family driving detail hydration strategy.
    /// </summary>
    public UserNotificationType TypeCode { get; private set; }

    /// <summary>
    /// Card and detail tag badge.
    /// </summary>
    public NotificationTag PrimaryTagCode { get; private set; }

    /// <summary>
    /// Role-based visibility group.
    /// </summary>
    public NotificationAudienceScope AudienceScopeCode { get; private set; }

    /// <summary>
    /// List and detail headline.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// List preview text.
    /// </summary>
    public string SummaryText { get; private set; } = null!;

    /// <summary>
    /// Optional stored body for informational notification types.
    /// </summary>
    public string? BodyText { get; private set; }

    /// <summary>
    /// Stable deduplication key unique per user.
    /// </summary>
    public string SourceEventKey { get; private set; } = null!;

    /// <summary>
    /// Source aggregate discriminator for hydration and actions.
    /// </summary>
    public string? RelatedAggregateType { get; private set; }

    /// <summary>
    /// Source aggregate identifier for hydration and actions.
    /// </summary>
    public Guid? RelatedAggregateId { get; private set; }

    /// <summary>
    /// Optional internal follow-up route.
    /// </summary>
    public string? DeepLinkPath { get; private set; }

    /// <summary>
    /// Whether the detail panel should render decision controls.
    /// </summary>
    public bool RequiresAction { get; private set; }

    /// <summary>
    /// Resolution badge synchronized from source domains when applicable.
    /// </summary>
    public NotificationActionState ActionState { get; private set; }

    /// <summary>
    /// Unread marker for list and header badge.
    /// </summary>
    public bool IsRead { get; private set; }

    /// <summary>
    /// First-read timestamp; remains null while unread.
    /// </summary>
    public DateTime? ReadAtUtc { get; private set; }

    // Relationships

    /// <summary>
    /// Optional announcement source for fan-out rows. One-to-Many from SystemAnnouncement.
    /// </summary>
    public SystemAnnouncement? SystemAnnouncement { get; private set; }

    private UserNotification() : base()
    {
    }

    private UserNotification(
        Guid userId,
        Guid? systemAnnouncementId,
        UserNotificationType typeCode,
        NotificationTag primaryTagCode,
        NotificationAudienceScope audienceScopeCode,
        string title,
        string summaryText,
        string? bodyText,
        string sourceEventKey,
        string? relatedAggregateType,
        Guid? relatedAggregateId,
        string? deepLinkPath,
        bool requiresAction,
        NotificationActionState actionState) : this()
    {
        UserId = userId;
        SystemAnnouncementId = systemAnnouncementId;
        TypeCode = typeCode;
        PrimaryTagCode = primaryTagCode;
        AudienceScopeCode = audienceScopeCode;
        Title = title;
        SummaryText = summaryText;
        BodyText = bodyText;
        SourceEventKey = sourceEventKey;
        RelatedAggregateType = relatedAggregateType;
        RelatedAggregateId = relatedAggregateId;
        DeepLinkPath = deepLinkPath;
        RequiresAction = requiresAction;
        ActionState = actionState;
        IsRead = false;
        ReadAtUtc = null;
    }

    /// <summary>
    /// Creates a projection row from a normalized draft with invariant checks.
    /// </summary>
    public static ErrorOr<UserNotification> Create(NotificationProjectionDraft draft)
    {
        if (draft.UserId == Guid.Empty)
        {
            return Error.Validation(
                code: "UserNotification.UserIdEmpty",
                description: "User id must be provided.");
        }

        if (!Enum.IsDefined(draft.TypeCode)
            || !Enum.IsDefined(draft.PrimaryTagCode)
            || !Enum.IsDefined(draft.AudienceScopeCode))
        {
            return Error.Validation(
                code: "UserNotification.InvalidEnum",
                description: "Notification enum value is invalid.");
        }

        ErrorOr<string> sourceEventKey = NormalizeRequiredText(draft.SourceEventKey, 256, "sourceEventKey");
        if (sourceEventKey.IsError)
        {
            return sourceEventKey.Errors;
        }

        ErrorOr<string> title = NormalizeRequiredText(draft.Title, 200, "title");
        if (title.IsError)
        {
            return title.Errors;
        }

        ErrorOr<string> summaryText = NormalizeRequiredText(draft.SummaryText, 500, "summaryText");
        if (summaryText.IsError)
        {
            return summaryText.Errors;
        }

        string? bodyText = NormalizeOptionalText(draft.BodyText, int.MaxValue);
        string? relatedAggregateType = NormalizeOptionalText(draft.RelatedAggregateType, 64);
        string? deepLinkPath = NormalizeOptionalText(draft.DeepLinkPath, 512);

        ErrorOr<Success> requiresActionValidation = ValidateRequiresActionCombination(
            draft.TypeCode,
            draft.RequiresAction);
        if (requiresActionValidation.IsError)
        {
            return requiresActionValidation.Errors;
        }

        NotificationActionState actionState = draft.ActionState
            ?? (draft.RequiresAction
                ? NotificationActionState.Pending
                : NotificationActionState.Informational);

        if (!Enum.IsDefined(actionState))
        {
            return Error.Validation(
                code: "UserNotification.InvalidActionState",
                description: "Action state is invalid.");
        }

        return new UserNotification(
            draft.UserId,
            draft.SystemAnnouncementId,
            draft.TypeCode,
            draft.PrimaryTagCode,
            draft.AudienceScopeCode,
            title.Value,
            summaryText.Value,
            bodyText,
            sourceEventKey.Value,
            relatedAggregateType,
            draft.RelatedAggregateId,
            deepLinkPath,
            draft.RequiresAction,
            actionState);
    }

    /// <summary>
    /// Applies idempotent read-state transition.
    /// </summary>
    public ErrorOr<Success> MarkAsRead(DateTime readAtUtc)
    {
        if (IsRead)
        {
            return Result.Success;
        }

        IsRead = true;
        ReadAtUtc = readAtUtc;
        return Result.Success;
    }

    /// <summary>
    /// Updates resolution badge when the source domain resolves elsewhere.
    /// </summary>
    public ErrorOr<Success> SyncActionState(NotificationActionState actionState, bool requiresAction)
    {
        if (!Enum.IsDefined(actionState))
        {
            return Error.Validation(
                code: "UserNotification.InvalidActionState",
                description: "Action state is invalid.");
        }

        ActionState = actionState;
        RequiresAction = requiresAction;
        return Result.Success;
    }

    /// <summary>
    /// Updates denormalized projection fields without resetting read state.
    /// </summary>
    public ErrorOr<Success> UpdateProjection(
        string title,
        string summaryText,
        string? bodyText,
        string? deepLinkPath,
        bool requiresAction,
        NotificationActionState actionState,
        string? relatedAggregateType,
        Guid? relatedAggregateId)
    {
        ErrorOr<string> normalizedTitle = NormalizeRequiredText(title, 200, "title");
        if (normalizedTitle.IsError)
        {
            return normalizedTitle.Errors;
        }

        ErrorOr<string> normalizedSummary = NormalizeRequiredText(summaryText, 500, "summaryText");
        if (normalizedSummary.IsError)
        {
            return normalizedSummary.Errors;
        }

        if (!Enum.IsDefined(actionState))
        {
            return Error.Validation(
                code: "UserNotification.InvalidActionState",
                description: "Action state is invalid.");
        }

        Title = normalizedTitle.Value;
        SummaryText = normalizedSummary.Value;
        BodyText = NormalizeOptionalText(bodyText, int.MaxValue);
        DeepLinkPath = NormalizeOptionalText(deepLinkPath, 512);
        RequiresAction = requiresAction;
        ActionState = actionState;
        RelatedAggregateType = NormalizeOptionalText(relatedAggregateType, 64);
        RelatedAggregateId = relatedAggregateId;
        return Result.Success;
    }

    private static ErrorOr<Success> ValidateRequiresActionCombination(
        UserNotificationType typeCode,
        bool requiresAction)
    {
        bool actionType = typeCode is UserNotificationType.SupervisionRequest
            or UserNotificationType.MedicalChangeProposal;

        if (requiresAction && !actionType)
        {
            return Error.Validation(
                code: "UserNotification.InvalidRequiresAction",
                description: "RequiresAction is only supported for request and proposal types in the initial scope.");
        }

        return Result.Success;
    }

    private static ErrorOr<string> NormalizeRequiredText(string value, int maxLength, string field)
    {
        string trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Error.Validation(
                code: "UserNotification.TextRequired",
                description: "Text value must be provided.",
                metadata: new()
                {
                    ["field"] = field
                });
        }

        if (trimmed.Length > maxLength)
        {
            return Error.Validation(
                code: "UserNotification.TextTooLong",
                description: "Text value exceeds maximum length.",
                metadata: new()
                {
                    ["field"] = field,
                    ["maxLength"] = maxLength
                });
        }

        return trimmed;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}
