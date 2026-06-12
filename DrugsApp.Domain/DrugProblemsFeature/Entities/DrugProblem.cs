using System;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.DrugFeature.Entities;
using DrugsApp.Domain.DrugProblemsFeature.Enums;
using ErrorOr;

namespace DrugsApp.Domain.DrugProblemsFeature.Entities;

/// <summary>
/// User-owned drug problem record that may reference a catalog drug or store a custom drug name.
/// </summary>
public sealed class DrugProblem : BaseEntity
{
    // Relationships

    /// <summary>
    /// Optional catalog drug form reference when <see cref="Source"/> is <see cref="DrugProblemSource.Database"/>.
    /// </summary>
    public DrugForm? DrugForm { get; private set; }

    // Attributes

    /// <summary>
    /// Owner identity derived from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog drug form identifier when <see cref="Source"/> is <see cref="DrugProblemSource.Database"/>.
    /// </summary>
    public Guid? DrugFormId { get; private set; }

    /// <summary>
    /// Custom user-provided drug name when <see cref="Source"/> is <see cref="DrugProblemSource.Custom"/>.
    /// </summary>
    public string? CustomDrugName { get; private set; }

    /// <summary>
    /// Custom active substances description when <see cref="Source"/> is <see cref="DrugProblemSource.Custom"/>.
    /// </summary>
    public string? CustomActiveSubstances { get; private set; }

    /// <summary>
    /// Custom formulation name when <see cref="Source"/> is <see cref="DrugProblemSource.Custom"/>.
    /// </summary>
    public string? CustomFormulationName { get; private set; }

    /// <summary>
    /// Custom dosage description when <see cref="Source"/> is <see cref="DrugProblemSource.Custom"/>.
    /// </summary>
    public string? CustomDosage { get; private set; }

    /// <summary>
    /// Indicates whether the record references a catalog drug or a custom name.
    /// </summary>
    public DrugProblemSource Source { get; private set; }

    /// <summary>
    /// Free-text note describing the problem experienced with the drug.
    /// </summary>
    public string ProblemNote { get; private set; } = null!;

    private DrugProblem() : base()
    {
    }

    private DrugProblem(
        Guid userId,
        Guid? drugFormId,
        string? customDrugName,
        string? customActiveSubstances,
        string? customFormulationName,
        string? customDosage,
        DrugProblemSource source,
        string problemNote) : this()
    {
        UserId = userId;
        DrugFormId = drugFormId;
        CustomDrugName = customDrugName;
        CustomActiveSubstances = customActiveSubstances;
        CustomFormulationName = customFormulationName;
        CustomDosage = customDosage;
        Source = source;
        ProblemNote = problemNote;
    }

    /// <summary>
    /// Creates a catalog-backed drug problem entry.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="drugFormId">Catalog drug form identifier.</param>
    /// <param name="problemNote">Problem note.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<DrugProblem> CreateDatabase(Guid userId, Guid drugFormId, string problemNote)
    {
        ErrorOr<(Guid UserId, Guid DrugFormId)> ownership = ValidateOwnership(userId, drugFormId);
        if (ownership.IsError)
        {
            return ownership.Errors;
        }

        ErrorOr<string> normalizedNote = NormalizeNote(problemNote);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new DrugProblem(
            userId: ownership.Value.UserId,
            drugFormId: ownership.Value.DrugFormId,
            customDrugName: null,
            customActiveSubstances: null,
            customFormulationName: null,
            customDosage: null,
            source: DrugProblemSource.Database,
            problemNote: normalizedNote.Value);
    }

    /// <summary>
    /// Creates a custom drug problem entry.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="customDrugName">Custom drug name.</param>
    /// <param name="customActiveSubstances">Custom active substances description.</param>
    /// <param name="customFormulationName">Custom formulation name.</param>
    /// <param name="customDosage">Custom dosage.</param>
    /// <param name="problemNote">Problem note.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<DrugProblem> CreateCustom(
        Guid userId,
        string customDrugName,
        string customActiveSubstances,
        string customFormulationName,
        string customDosage,
        string problemNote)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "DrugProblem.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        ErrorOr<string> normalizedCustomDrugName = NormalizeCustomDrugName(customDrugName);
        if (normalizedCustomDrugName.IsError)
        {
            return normalizedCustomDrugName.Errors;
        }

        ErrorOr<string> normalizedActiveSubstances = NormalizeRequiredString(customActiveSubstances, "customActiveSubstances", 300);
        if (normalizedActiveSubstances.IsError)
        {
            return normalizedActiveSubstances.Errors;
        }

        ErrorOr<string> normalizedFormulationName = NormalizeRequiredString(customFormulationName, "customFormulationName", 300);
        if (normalizedFormulationName.IsError)
        {
            return normalizedFormulationName.Errors;
        }

        ErrorOr<string> normalizedDosage = NormalizeRequiredString(customDosage, "customDosage", 200);
        if (normalizedDosage.IsError)
        {
            return normalizedDosage.Errors;
        }

        ErrorOr<string> normalizedNote = NormalizeNote(problemNote);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new DrugProblem(
            userId: userId,
            drugFormId: null,
            customDrugName: normalizedCustomDrugName.Value,
            customActiveSubstances: normalizedActiveSubstances.Value,
            customFormulationName: normalizedFormulationName.Value,
            customDosage: normalizedDosage.Value,
            source: DrugProblemSource.Custom,
            problemNote: normalizedNote.Value);
    }

    /// <summary>
    /// Updates the problem note for the record.
    /// </summary>
    /// <param name="problemNote">New note value.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateProblemNote(string problemNote)
    {
        ErrorOr<string> normalizedNote = NormalizeNote(problemNote);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        if (string.Equals(ProblemNote, normalizedNote.Value, StringComparison.Ordinal))
        {
            return Result.Success;
        }

        ProblemNote = normalizedNote.Value;
        return Result.Success;
    }

    /// <summary>
    /// Updates the catalog drug for database-sourced records.
    /// </summary>
    /// <param name="drugFormId">New catalog drug form identifier.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateDatabaseDrug(Guid drugFormId)
    {
        if (Source != DrugProblemSource.Database)
        {
            return Error.Validation(
                code: "DrugProblem.SourceMismatch",
                description: "Drug reference can be changed only for catalog-backed records.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                    ["reason"] = "sourceMismatch",
                });
        }

        if (drugFormId == Guid.Empty)
        {
            return Error.Validation(
                code: "DrugProblem.DrugFormIdEmpty",
                description: "Drug form id must be provided.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                });
        }

        if (DrugFormId == drugFormId)
        {
            return Result.Success;
        }

        DrugFormId = drugFormId;
        return Result.Success;
    }

    /// <summary>
    /// Updates the custom drug name for custom-sourced records.
    /// </summary>
    /// <param name="customDrugName">New custom name.</param>
    /// <param name="customActiveSubstances">Updated custom active substances.</param>
    /// <param name="customFormulationName">Updated custom formulation.</param>
    /// <param name="customDosage">Updated custom dosage.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateCustomDrugName(
        string customDrugName,
        string customActiveSubstances,
        string customFormulationName,
        string customDosage)
    {
        if (Source != DrugProblemSource.Custom)
        {
            return Error.Validation(
                code: "DrugProblem.SourceMismatch",
                description: "Custom drug name can be changed only for custom records.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["reason"] = "sourceMismatch",
                });
        }

        ErrorOr<string> normalizedName = NormalizeCustomDrugName(customDrugName);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<string> normalizedActiveSubstances = NormalizeRequiredString(customActiveSubstances, "customActiveSubstances", 300);
        if (normalizedActiveSubstances.IsError)
        {
            return normalizedActiveSubstances.Errors;
        }

        ErrorOr<string> normalizedFormulationName = NormalizeRequiredString(customFormulationName, "customFormulationName", 300);
        if (normalizedFormulationName.IsError)
        {
            return normalizedFormulationName.Errors;
        }

        ErrorOr<string> normalizedDosage = NormalizeRequiredString(customDosage, "customDosage", 200);
        if (normalizedDosage.IsError)
        {
            return normalizedDosage.Errors;
        }

        bool unchanged =
            string.Equals(CustomDrugName, normalizedName.Value, StringComparison.Ordinal) &&
            string.Equals(CustomActiveSubstances, normalizedActiveSubstances.Value, StringComparison.Ordinal) &&
            string.Equals(CustomFormulationName, normalizedFormulationName.Value, StringComparison.Ordinal) &&
            string.Equals(CustomDosage, normalizedDosage.Value, StringComparison.Ordinal);

        if (unchanged)
        {
            return Result.Success;
        }

        CustomDrugName = normalizedName.Value;
        CustomActiveSubstances = normalizedActiveSubstances.Value;
        CustomFormulationName = normalizedFormulationName.Value;
        CustomDosage = normalizedDosage.Value;
        return Result.Success;
    }

    /// <summary>
    /// Attaches the catalog drug navigation after loading.
    /// </summary>
    /// <param name="drugForm">Catalog drug form to attach.</param>
    public void AttachDrugForm(DrugForm drugForm)
    {
        DrugForm = drugForm;
        DrugFormId = drugForm.Id;
    }

    private static ErrorOr<(Guid UserId, Guid DrugFormId)> ValidateOwnership(Guid userId, Guid drugFormId)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "DrugProblem.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        if (drugFormId == Guid.Empty)
        {
            return Error.Validation(
                code: "DrugProblem.DrugFormIdEmpty",
                description: "Drug form id must be provided.",
                metadata: new()
                {
                    ["field"] = "drugFormId",
                });
        }

        return (userId, drugFormId);
    }

    private static ErrorOr<string> NormalizeCustomDrugName(string value)
    {
        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: "DrugProblem.CustomDrugNameEmpty",
                description: "Custom drug name must be provided.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                });
        }

        if (collapsed.Length > 300)
        {
            return Error.Validation(
                code: "DrugProblem.CustomDrugNameTooLong",
                description: "Custom drug name must not exceed 300 characters.",
                metadata: new()
                {
                    ["field"] = "customDrugName",
                    ["maxLength"] = 300,
                });
        }

        return collapsed;
    }

    private static ErrorOr<string> NormalizeNote(string value)
    {
        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: "DrugProblem.ProblemNoteEmpty",
                description: "Problem note must be provided.",
                metadata: new()
                {
                    ["field"] = "problemNote",
                });
        }

        if (collapsed.Length > 2000)
        {
            return Error.Validation(
                code: "DrugProblem.ProblemNoteTooLong",
                description: "Problem note must not exceed 2000 characters.",
                metadata: new()
                {
                    ["field"] = "problemNote",
                    ["maxLength"] = 2000,
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

    private static ErrorOr<string> NormalizeRequiredString(string value, string field, int maxLength)
    {
        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: $"DrugProblem.{field}Empty",
                description: $"{field} must be provided.",
                metadata: new()
                {
                    ["field"] = field,
                });
        }

        if (collapsed.Length > maxLength)
        {
            return Error.Validation(
                code: $"DrugProblem.{field}TooLong",
                description: $"{field} must not exceed {maxLength} characters.",
                metadata: new()
                {
                    ["field"] = field,
                    ["maxLength"] = maxLength,
                });
        }

        return collapsed;
    }
}
