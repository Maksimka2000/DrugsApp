using System;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.SupplementFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.AllergiesFeature.Entities;

/// <summary>
/// Per-user supplement allergy record referencing a catalog supplement or a custom name.
/// </summary>
public sealed class ClientSupplementAllergy : BaseEntity
{
    // Relationships

    /// <summary>
    /// Optional catalog supplement reference when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public Supplement? Supplement { get; private set; }

    // Attributes

    /// <summary>
    /// Owner identifier from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog supplement identifier when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public Guid? SupplementId { get; private set; }

    /// <summary>
    /// Custom user-provided supplement name when <see cref="Source"/> is <see cref="AllergyRecordSource.Custom"/>.
    /// </summary>
    public string? CustomSupplementName { get; private set; }

    /// <summary>
    /// Normalized custom name for duplicate checks.
    /// </summary>
    public string? CustomSupplementNameNormalized { get; private set; }

    /// <summary>
    /// Record origin.
    /// </summary>
    public AllergyRecordSource Source { get; private set; }

    /// <summary>
    /// Optional note describing the allergy.
    /// </summary>
    public string? Note { get; private set; }

    private ClientSupplementAllergy() : base()
    {
    }

    private ClientSupplementAllergy(
        Guid userId,
        Guid? supplementId,
        string? customSupplementName,
        string? customSupplementNameNormalized,
        AllergyRecordSource source,
        string? note) : this()
    {
        UserId = userId;
        SupplementId = supplementId;
        CustomSupplementName = customSupplementName;
        CustomSupplementNameNormalized = customSupplementNameNormalized;
        Source = source;
        Note = note;
    }

    /// <summary>
    /// Creates a catalog-backed supplement allergy.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="supplementId">Catalog supplement identifier.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientSupplementAllergy> CreateDatabase(Guid userId, Guid supplementId, string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientSupplementAllergy.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        if (supplementId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientSupplementAllergy.SupplementIdEmpty",
                description: "Supplement id must be provided.",
                metadata: new()
                {
                    ["field"] = "supplementId",
                });
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientSupplementAllergy(
            userId: userId,
            supplementId: supplementId,
            customSupplementName: null,
            customSupplementNameNormalized: null,
            source: AllergyRecordSource.Database,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Creates a custom-name supplement allergy.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="customSupplementName">Custom supplement name.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientSupplementAllergy> CreateCustom(Guid userId, string customSupplementName, string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientSupplementAllergy.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        ErrorOr<(string Display, string Normalized)> normalizedName = NormalizeName(customSupplementName, "customSupplementName");
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientSupplementAllergy(
            userId: userId,
            supplementId: null,
            customSupplementName: normalizedName.Value.Display,
            customSupplementNameNormalized: normalizedName.Value.Normalized,
            source: AllergyRecordSource.Custom,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Updates the stored note, trimming or clearing when empty.
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

        if (string.Equals(Note, normalizedNote.Value, StringComparison.Ordinal))
        {
            return Result.Success;
        }

        Note = normalizedNote.Value;
        return Result.Success;
    }

    private static ErrorOr<(string Display, string Normalized)> NormalizeName(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Error.Validation(
                code: "ClientSupplementAllergy.NameEmpty",
                description: "Name must not be empty.",
                metadata: new()
                {
                    ["field"] = field,
                });
        }

        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: "ClientSupplementAllergy.NameEmpty",
                description: "Name must not be empty.",
                metadata: new()
                {
                    ["field"] = field,
                });
        }

        return (collapsed, collapsed.ToLowerInvariant());
    }

    private static ErrorOr<string?> NormalizeNote(string? value)
    {
        if (value is null)
        {
            return (string?)null;
        }

        string trimmed = value.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? (string?)null : trimmed;
    }

    private static string CollapseWhitespace(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
            ' ',
            trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
