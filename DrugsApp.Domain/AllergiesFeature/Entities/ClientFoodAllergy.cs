using System;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.FoodFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.AllergiesFeature.Entities;

/// <summary>
/// Per-user food allergy record referencing a catalog food or a custom name.
/// </summary>
public sealed class ClientFoodAllergy : BaseEntity
{
    // Relationships

    /// <summary>
    /// Optional catalog food reference when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public Food? Food { get; private set; }

    // Attributes

    /// <summary>
    /// Owner identifier from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog food identifier when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public Guid? FoodId { get; private set; }

    /// <summary>
    /// Custom user-provided food name when <see cref="Source"/> is <see cref="AllergyRecordSource.Custom"/>.
    /// </summary>
    public string? CustomFoodName { get; private set; }

    /// <summary>
    /// Normalized custom name for duplicate checks.
    /// </summary>
    public string? CustomFoodNameNormalized { get; private set; }

    /// <summary>
    /// Record origin.
    /// </summary>
    public AllergyRecordSource Source { get; private set; }

    /// <summary>
    /// Optional note describing the allergy.
    /// </summary>
    public string? Note { get; private set; }

    private ClientFoodAllergy() : base()
    {
    }

    private ClientFoodAllergy(
        Guid userId,
        Guid? foodId,
        string? customFoodName,
        string? customFoodNameNormalized,
        AllergyRecordSource source,
        string? note) : this()
    {
        UserId = userId;
        FoodId = foodId;
        CustomFoodName = customFoodName;
        CustomFoodNameNormalized = customFoodNameNormalized;
        Source = source;
        Note = note;
    }

    /// <summary>
    /// Creates a catalog-backed food allergy.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="foodId">Catalog food identifier.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientFoodAllergy> CreateDatabase(Guid userId, Guid foodId, string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientFoodAllergy.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        if (foodId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientFoodAllergy.FoodIdEmpty",
                description: "Food id must be provided.",
                metadata: new()
                {
                    ["field"] = "foodId",
                });
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientFoodAllergy(
            userId: userId,
            foodId: foodId,
            customFoodName: null,
            customFoodNameNormalized: null,
            source: AllergyRecordSource.Database,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Creates a custom-name food allergy.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="customFoodName">Custom food name.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientFoodAllergy> CreateCustom(Guid userId, string customFoodName, string? note)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientFoodAllergy.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        ErrorOr<(string Display, string Normalized)> normalizedName = NormalizeName(customFoodName, "customFoodName");
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientFoodAllergy(
            userId: userId,
            foodId: null,
            customFoodName: normalizedName.Value.Display,
            customFoodNameNormalized: normalizedName.Value.Normalized,
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
                code: "ClientFoodAllergy.NameEmpty",
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
                code: "ClientFoodAllergy.NameEmpty",
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
