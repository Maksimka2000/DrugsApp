using System;
using DrugsApp.Domain.AllergiesFeature.Enums;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.DrugFeature.Entities;
using ErrorOr;

namespace DrugsApp.Domain.AllergiesFeature.Entities;

/// <summary>
/// Per-user drug allergy record referencing a catalog drug form or a custom drug description.
/// </summary>
public sealed class ClientDrugAllergy : BaseEntity
{
    // Relationships

    /// <summary>
    /// Optional catalog drug form reference when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public DrugForm? DrugForm { get; private set; }

    // Attributes

    /// <summary>
    /// Owner identifier from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog drug form identifier when <see cref="Source"/> is <see cref="AllergyRecordSource.Database"/>.
    /// </summary>
    public Guid? DrugFormId { get; private set; }

    /// <summary>
    /// Custom user-provided drug name when <see cref="Source"/> is <see cref="AllergyRecordSource.Custom"/>.
    /// </summary>
    public string? CustomDrugName { get; private set; }

    /// <summary>
    /// Normalized custom name for duplicate checks.
    /// </summary>
    public string? CustomDrugNameNormalized { get; private set; }

    /// <summary>
    /// Custom active substances description when <see cref="Source"/> is <see cref="AllergyRecordSource.Custom"/>.
    /// </summary>
    public string? CustomActiveSubstances { get; private set; }

    /// <summary>
    /// Custom formulation name when <see cref="Source"/> is <see cref="AllergyRecordSource.Custom"/>.
    /// </summary>
    public string? CustomFormulationName { get; private set; }

    /// <summary>
    /// Custom dosage description when <see cref="Source"/> is <see cref="AllergyRecordSource.Custom"/>.
    /// </summary>
    public string? CustomDosage { get; private set; }

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
        Guid? drugFormId,
        string? customDrugName,
        string? customDrugNameNormalized,
        string? customActiveSubstances,
        string? customFormulationName,
        string? customDosage,
        AllergyRecordSource source,
        string? note) : this()
    {
        UserId = userId;
        DrugFormId = drugFormId;
        CustomDrugName = customDrugName;
        CustomDrugNameNormalized = customDrugNameNormalized;
        CustomActiveSubstances = customActiveSubstances;
        CustomFormulationName = customFormulationName;
        CustomDosage = customDosage;
        Source = source;
        Note = note;
    }

    /// <summary>
    /// Creates a catalog-backed drug allergy linked to a drug form.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="drugFormId">Catalog drug form identifier.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientDrugAllergy> CreateDatabase(Guid userId, Guid drugFormId, string? note)
    {
        ErrorOr<(Guid UserId, Guid DrugFormId)> ownership = ValidateOwnership(userId, drugFormId);
        if (ownership.IsError)
        {
            return ownership.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientDrugAllergy(
            userId: ownership.Value.UserId,
            drugFormId: ownership.Value.DrugFormId,
            customDrugName: null,
            customDrugNameNormalized: null,
            customActiveSubstances: null,
            customFormulationName: null,
            customDosage: null,
            source: AllergyRecordSource.Database,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Creates a custom drug allergy with user-provided drug details.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="customDrugName">Custom drug name.</param>
    /// <param name="customActiveSubstances">Custom active substances description.</param>
    /// <param name="customFormulationName">Custom formulation name.</param>
    /// <param name="customDosage">Custom dosage.</param>
    /// <param name="note">Optional note.</param>
    /// <returns>Validated aggregate or errors.</returns>
    public static ErrorOr<ClientDrugAllergy> CreateCustom(
        Guid userId,
        string customDrugName,
        string customActiveSubstances,
        string customFormulationName,
        string customDosage,
        string? note)
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

        ErrorOr<string> normalizedActiveSubstances = NormalizeRequiredString(
            customActiveSubstances,
            "customActiveSubstances",
            300);
        if (normalizedActiveSubstances.IsError)
        {
            return normalizedActiveSubstances.Errors;
        }

        ErrorOr<string> normalizedFormulationName = NormalizeRequiredString(
            customFormulationName,
            "customFormulationName",
            300);
        if (normalizedFormulationName.IsError)
        {
            return normalizedFormulationName.Errors;
        }

        ErrorOr<string> normalizedDosage = NormalizeRequiredString(customDosage, "customDosage", 200);
        if (normalizedDosage.IsError)
        {
            return normalizedDosage.Errors;
        }

        ErrorOr<string?> normalizedNote = NormalizeNote(note);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new ClientDrugAllergy(
            userId: userId,
            drugFormId: null,
            customDrugName: normalizedName.Value.Display,
            customDrugNameNormalized: normalizedName.Value.Normalized,
            customActiveSubstances: normalizedActiveSubstances.Value,
            customFormulationName: normalizedFormulationName.Value,
            customDosage: normalizedDosage.Value,
            source: AllergyRecordSource.Custom,
            note: normalizedNote.Value);
    }

    /// <summary>
    /// Attaches the catalog drug form navigation after loading.
    /// </summary>
    /// <param name="drugForm">Catalog drug form to attach.</param>
    public void AttachDrugForm(DrugForm drugForm)
    {
        DrugForm = drugForm;
        DrugFormId = drugForm.Id;
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

    private static ErrorOr<(Guid UserId, Guid DrugFormId)> ValidateOwnership(Guid userId, Guid drugFormId)
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

        if (drugFormId == Guid.Empty)
        {
            return Error.Validation(
                code: "ClientDrugAllergy.DrugFormIdEmpty",
                description: "Drug form id must be provided.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                });
        }

        return (userId, drugFormId);
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

        if (collapsed.Length > 300)
        {
            return Error.Validation(
                code: "ClientDrugAllergy.NameTooLong",
                description: "Name must not exceed 300 characters.",
                metadata: new()
                {
                    ["field"] = field,
                    ["maxLength"] = 300,
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
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return (string?)null;
        }

        if (trimmed.Length > 1000)
        {
            return Error.Validation(
                code: "ClientDrugAllergy.NoteTooLong",
                description: "Note must not exceed 1000 characters.",
                metadata: new()
                {
                    ["field"] = "note",
                    ["maxLength"] = 1000,
                });
        }

        return trimmed;
    }

    private static ErrorOr<string> NormalizeRequiredString(string value, string field, int maxLength)
    {
        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: $"ClientDrugAllergy.{field}Empty",
                description: $"{field} must be provided.",
                metadata: new()
                {
                    ["field"] = field,
                });
        }

        if (collapsed.Length > maxLength)
        {
            return Error.Validation(
                code: $"ClientDrugAllergy.{field}TooLong",
                description: $"{field} must not exceed {maxLength} characters.",
                metadata: new()
                {
                    ["field"] = field,
                    ["maxLength"] = maxLength,
                });
        }

        return collapsed;
    }

    private static string CollapseWhitespace(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
            ' ',
            trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
