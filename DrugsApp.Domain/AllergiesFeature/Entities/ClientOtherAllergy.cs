using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.AllergiesFeature.Entities;

/// <summary>
/// Per-user free-text allergy record (non-catalog).
/// </summary>
public sealed class ClientOtherAllergy : BaseEntity
{
    // Attributes

    /// <summary>
    /// Owner identifier from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Display name provided by the user.
    /// </summary>
    public string AllergyName { get; private set; } = null!;

    /// <summary>
    /// Normalized allergy name for duplicate checks.
    /// </summary>
    public string AllergyNameNormalized { get; private set; } = null!;

    /// <summary>
    /// Optional note describing the allergy.
    /// </summary>
    public string? Note { get; private set; }

    private ClientOtherAllergy() : base()
    {
    }

    private ClientOtherAllergy(
        Guid userId,
        string allergyName,
        string allergyNameNormalized,
        string? note) : this()
    {
        UserId = userId;
        AllergyName = allergyName;
        AllergyNameNormalized = allergyNameNormalized;
        Note = note;
    }

    /// <summary>
    /// Creates a new other-allergy record.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="allergyName">Allergy name.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientOtherAllergy> Create(Guid userId, string allergyName, string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientOtherAllergy.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        ErrorOr<(string Display, string Normalized)> normalizedName = NormalizeName(allergyName, "allergyName");
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientOtherAllergy(
            userId: userId,
            allergyName: normalizedName.Value.Display,
            allergyNameNormalized: normalizedName.Value.Normalized,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Updates the allergy name with normalization and duplicate-ready value.
    /// </summary>
    /// <param name="allergyName">New allergy name.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateName(string allergyName)
    {
        ErrorOr<(string Display, string Normalized)> normalizedName = NormalizeName(allergyName, "allergyName");
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        if (string.Equals(AllergyNameNormalized, normalizedName.Value.Normalized, StringComparison.Ordinal))
        {
            return Result.Success;
        }

        AllergyName = normalizedName.Value.Display;
        AllergyNameNormalized = normalizedName.Value.Normalized;
        return Result.Success;
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
                code: "ClientOtherAllergy.NameEmpty",
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
                code: "ClientOtherAllergy.NameEmpty",
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
