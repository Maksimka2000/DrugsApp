using System;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.DrugFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.AllergiesFeature.Entities;

/// <summary>
/// Per-user drug allergy record referencing a catalog drug or a custom name.
/// </summary>
public sealed class ClientDrugAllergy : BaseEntity
{
    // Relationships

    /// <summary>
    /// Optional catalog drug reference when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public Drug? Drug { get; private set; }

    // Attributes

    /// <summary>
    /// Owner identifier from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog drug identifier when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public Guid? DrugId { get; private set; }

    /// <summary>
    /// Custom user-provided drug name when <see cref="Source"/> is <see cref="AllergyRecordSource.Custom"/>.
    /// </summary>
    public string? CustomDrugName { get; private set; }

    /// <summary>
    /// Normalized custom name for duplicate checks.
    /// </summary>
    public string? CustomDrugNameNormalized { get; private set; }

    /// <summary>
    /// Record origin.
    /// </summary>
    public AllergyRecordSource Source { get; private set; }

    /// <summary>
    /// Optional note describing the allergy.
    /// </summary>
    public string? Note { get; private set; }

    private ClientDrugAllergy() : base()
    {
    }

    private ClientDrugAllergy(
        Guid userId,
        Guid? drugId,
        string? customDrugName,
        string? customDrugNameNormalized,
        AllergyRecordSource source,
        string? note) : this()
    {
        UserId = userId;
        DrugId = drugId;
        CustomDrugName = customDrugName;
        CustomDrugNameNormalized = customDrugNameNormalized;
        Source = source;
        Note = note;
    }

    /// <summary>
    /// Creates a catalog-backed drug allergy.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="drugId">Catalog drug identifier.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientDrugAllergy> CreateDatabase(Guid userId, Guid drugId, string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientDrugAllergy.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        if (drugId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientDrugAllergy.DrugIdEmpty",
                description: "Drug id must be provided.",
                metadata: new()
                {
                    ["field"] = "drugId",
                });
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientDrugAllergy(
            userId: userId,
            drugId: drugId,
            customDrugName: null,
            customDrugNameNormalized: null,
            source: AllergyRecordSource.Database,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Creates a custom-name drug allergy.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="customDrugName">Custom drug name.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientDrugAllergy> CreateCustom(Guid userId, string customDrugName, string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientDrugAllergy.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        ErrorOr<(string Display, string Normalized)> normalizedName = NormalizeName(customDrugName, "customDrugName");
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientDrugAllergy(
            userId: userId,
            drugId: null,
            customDrugName: normalizedName.Value.Display,
            customDrugNameNormalized: normalizedName.Value.Normalized,
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
                code: "ClientDrugAllergy.NameEmpty",
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
                code: "ClientDrugAllergy.NameEmpty",
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
