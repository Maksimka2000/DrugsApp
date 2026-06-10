using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.NotificationsFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.NotificationsFeature.Entities;

/// <summary>
/// Global system-authored announcement source record fanned out into user inbox rows.
/// </summary>
public sealed class SystemAnnouncement : BaseEntity
{
    // Attributes

    /// <summary>
    /// Audience visibility scope for fan-out.
    /// </summary>
    public AnnouncementAudienceScope AudienceScopeCode { get; private set; }

    /// <summary>
    /// Tag badge shown on inbox cards.
    /// </summary>
    public NotificationTag PrimaryTagCode { get; private set; }

    /// <summary>
    /// Short headline shown in list and detail views.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// List-card preview text.
    /// </summary>
    public string SummaryText { get; private set; } = null!;

    /// <summary>
    /// Full detail body text.
    /// </summary>
    public string BodyText { get; private set; } = null!;

    /// <summary>
    /// Optional internal app route for follow-up navigation.
    /// </summary>
    public string? DeepLinkPath { get; private set; }

    /// <summary>
    /// Publish start timestamp in UTC.
    /// </summary>
    public DateTime ActiveFromUtc { get; private set; }

    /// <summary>
    /// Optional publish end timestamp in UTC.
    /// </summary>
    public DateTime? ActiveToUtc { get; private set; }

    private SystemAnnouncement() : base()
    {
    }

    private SystemAnnouncement(
        AnnouncementAudienceScope audienceScopeCode,
        NotificationTag primaryTagCode,
        string title,
        string summaryText,
        string bodyText,
        string? deepLinkPath,
        DateTime activeFromUtc,
        DateTime? activeToUtc) : this()
    {
        AudienceScopeCode = audienceScopeCode;
        PrimaryTagCode = primaryTagCode;
        Title = title;
        SummaryText = summaryText;
        BodyText = bodyText;
        DeepLinkPath = deepLinkPath;
        ActiveFromUtc = activeFromUtc;
        ActiveToUtc = activeToUtc;
    }

    /// <summary>
    /// Creates a valid announcement row for seed or admin workflows.
    /// </summary>
    public static ErrorOr<SystemAnnouncement> Create(
        AnnouncementAudienceScope audienceScopeCode,
        NotificationTag primaryTagCode,
        string title,
        string summaryText,
        string bodyText,
        string? deepLinkPath,
        DateTime activeFromUtc,
        DateTime? activeToUtc)
    {
        if (!Enum.IsDefined(audienceScopeCode))
        {
            return Error.Validation(
                code: "SystemAnnouncement.InvalidAudienceScope",
                description: "Audience scope is invalid.");
        }

        if (!Enum.IsDefined(primaryTagCode))
        {
            return Error.Validation(
                code: "SystemAnnouncement.InvalidPrimaryTag",
                description: "Primary tag is invalid.");
        }

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

        ErrorOr<string> normalizedBody = NormalizeRequiredText(bodyText, int.MaxValue, "bodyText");
        if (normalizedBody.IsError)
        {
            return normalizedBody.Errors;
        }

        string? normalizedDeepLink = NormalizeOptionalText(deepLinkPath, 512);

        if (activeToUtc is not null && activeToUtc < activeFromUtc)
        {
            return Error.Validation(
                code: "SystemAnnouncement.InvalidActiveWindow",
                description: "Active end must be greater than or equal to active start.");
        }

        return new SystemAnnouncement(
            audienceScopeCode,
            primaryTagCode,
            normalizedTitle.Value,
            normalizedSummary.Value,
            normalizedBody.Value,
            normalizedDeepLink,
            activeFromUtc,
            activeToUtc);
    }

    private static ErrorOr<string> NormalizeRequiredText(string value, int maxLength, string field)
    {
        string trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Error.Validation(
                code: "SystemAnnouncement.TextRequired",
                description: "Text value must be provided.",
                metadata: new()
                {
                    ["field"] = field
                });
        }

        if (trimmed.Length > maxLength)
        {
            return Error.Validation(
                code: "SystemAnnouncement.TextTooLong",
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
