using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.HealthConditionFeature.Entities;

/// <summary>
/// User-owned health condition record that may link to a catalog entry or store a custom name.
/// </summary>
public sealed class UserHealthCondition : BaseEntity
{
    // Relationships

    /// <summary>
    /// Optional linked catalog health condition when <see cref="Source"/> is <see cref="HealthConditionSource.Database"/>.
    /// </summary>
    public HealthCondition? HealthCondition { get; private set; }

    // Attributes

    /// <summary>
    /// Owner identity derived from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Linked catalog health condition identifier when source is database.
    /// </summary>
    public Guid? HealthConditionId { get; private set; }

    /// <summary>
    /// Custom user-provided health condition name when source is custom.
    /// </summary>
    public string? CustomHealthConditionName { get; private set; }

    /// <summary>
    /// Lower-case collapsed lookup for custom names used for per-user uniqueness.
    /// </summary>
    public string? NormalizedCustomHealthConditionName { get; private set; }

    /// <summary>
    /// Indicates whether the record references a catalog entry or custom value.
    /// </summary>
    public HealthConditionSource Source { get; private set; }

    /// <summary>
    /// Optional user note stored as null when blank.
    /// </summary>
    public string? Note { get; private set; }

    private UserHealthCondition() : base()
    {
    }

    private UserHealthCondition(
        Guid userId,
        Guid? healthConditionId,
        string? customHealthConditionName,
        string? normalizedCustomHealthConditionName,
        HealthConditionSource source,
        string? note) : this()
    {
        UserId = userId;
        HealthConditionId = healthConditionId;
        CustomHealthConditionName = customHealthConditionName;
        NormalizedCustomHealthConditionName = normalizedCustomHealthConditionName;
        Source = source;
        Note = note;
    }

    /// <summary>
    /// Creates a catalog-linked user health condition record.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="healthConditionId">Catalog health condition identifier.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<UserHealthCondition> CreateDatabase(Guid userId, Guid healthConditionId, string? note)
    {
        ErrorOr<Success> ownershipValidation = ValidateOwnership(userId, healthConditionId);
        if (ownershipValidation.IsError)
        {
            return ownershipValidation.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new UserHealthCondition(
            userId: userId,
            healthConditionId: healthConditionId,
            customHealthConditionName: null,
            normalizedCustomHealthConditionName: null,
            source: HealthConditionSource.Database,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Creates a custom user health condition record.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="customHealthConditionName">Custom display name.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<UserHealthCondition> CreateCustom(
        Guid userId,
        string customHealthConditionName,
        string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "UserHealthCondition.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        ErrorOr<(string DisplayValue, string LookupValue)> normalizedName = NormalizeCustomName(customHealthConditionName);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new UserHealthCondition(
            userId: userId,
            healthConditionId: null,
            customHealthConditionName: normalizedName.Value.DisplayValue,
            normalizedCustomHealthConditionName: normalizedName.Value.LookupValue,
            source: HealthConditionSource.Custom,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Updates the user note using normalized storage semantics.
    /// </summary>
    /// <param name="note">New note value.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateNote(string? note)
    {
        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        Note = normalizedNote.Value;
        return Result.Success;
    }

    /// <summary>
    /// Attaches the catalog health condition navigation after creation.
    /// </summary>
    /// <param name="healthCondition">Catalog entity to attach.</param>
    public void AttachHealthCondition(HealthCondition healthCondition)
    {
        HealthCondition = healthCondition;
        HealthConditionId = healthCondition.Id;
    }

    private static ErrorOr<Success> ValidateOwnership(Guid userId, Guid healthConditionId)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "UserHealthCondition.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        if (healthConditionId == Guid.Empty)
        {
            return Error.Validation(
                code: "UserHealthCondition.HealthConditionIdEmpty",
                description: "Health condition id must be provided.",
                metadata: new()
                {
                    ["field"] = "healthConditionId"
                });
        }

        return Result.Success;
    }

    private static ErrorOr<(string DisplayValue, string LookupValue)> NormalizeCustomName(string value)
    {
        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: "UserHealthCondition.CustomNameEmpty",
                description: "Custom health condition name must be provided.",
                metadata: new()
                {
                    ["field"] = "customHealthConditionName"
                });
        }

        if (collapsed.Length > 256)
        {
            return Error.Validation(
                code: "UserHealthCondition.CustomNameTooLong",
                description: "Custom health condition name must not exceed 256 characters.",
                metadata: new()
                {
                    ["field"] = "customHealthConditionName",
                    ["maxLength"] = 256
                });
        }

        return (collapsed, collapsed.ToLowerInvariant());
    }

    private static ErrorOr<string?> NormalizeNote(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (string?)null;
        }

        string trimmed = value.Trim();
        if (trimmed.Length > 2000)
        {
            return Error.Validation(
                code: "UserHealthCondition.NoteTooLong",
                description: "Note must not exceed 2000 characters.",
                metadata: new()
                {
                    ["field"] = "note",
                    ["maxLength"] = 2000
                });
        }

        return trimmed;
    }

    private static string CollapseWhitespace(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
