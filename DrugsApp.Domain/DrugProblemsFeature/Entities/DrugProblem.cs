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
    /// Optional catalog drug reference when <see cref="Source"/> is <see cref="DrugProblemSource.Database"/>.
    /// </summary>
    public Drug? Drug { get; private set; }

    // Attributes

    /// <summary>
    /// Owner identity derived from JWT subject.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Catalog drug identifier when <see cref="Source"/> is <see cref="DrugProblemSource.Database"/>.
    /// </summary>
    public Guid? DrugId { get; private set; }

    /// <summary>
    /// Custom user-provided drug name when <see cref="Source"/> is <see cref="DrugProblemSource.Custom"/>.
    /// </summary>
    public string? CustomDrugName { get; private set; }

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
        Guid? drugId,
        string? customDrugName,
        DrugProblemSource source,
        string problemNote) : this()
    {
        UserId = userId;
        DrugId = drugId;
        CustomDrugName = customDrugName;
        Source = source;
        ProblemNote = problemNote;
    }

    /// <summary>
    /// Creates a catalog-backed drug problem entry.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="drugId">Catalog drug identifier.</param>
    /// <param name="problemNote">Problem note.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<DrugProblem> CreateDatabase(Guid userId, Guid drugId, string problemNote)
    {
        ErrorOr<(Guid UserId, Guid DrugId)> ownership = ValidateOwnership(userId, drugId);
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
            drugId: ownership.Value.DrugId,
            customDrugName: null,
            source: DrugProblemSource.Database,
            problemNote: normalizedNote.Value);
    }

    /// <summary>
    /// Creates a custom drug problem entry.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="customDrugName">Custom drug name.</param>
    /// <param name="problemNote">Problem note.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<DrugProblem> CreateCustom(Guid userId, string customDrugName, string problemNote)
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

        ErrorOr<string> normalizedNote = NormalizeNote(problemNote);
        if (normalizedNote.IsError)
        {
            return normalizedNote.Errors;
        }

        return new DrugProblem(
            userId: userId,
            drugId: null,
            customDrugName: normalizedCustomDrugName.Value,
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
    /// <param name="drugId">New catalog drug identifier.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateDatabaseDrug(Guid drugId)
    {
        if (Source != DrugProblemSource.Database)
        {
            return Error.Validation(
                code: "DrugProblem.SourceMismatch",
                description: "Drug reference can be changed only for catalog-backed records.",
                metadata: new()
                {
                    ["field"] = "drugId",
                    ["reason"] = "sourceMismatch",
                });
        }

        if (drugId == Guid.Empty)
        {
            return Error.Validation(
                code: "DrugProblem.DrugIdEmpty",
                description: "Drug id must be provided.",
                metadata: new()
                {
                    ["field"] = "drugId",
                });
        }

        if (DrugId == drugId)
        {
            return Result.Success;
        }

        DrugId = drugId;
        return Result.Success;
    }

    /// <summary>
    /// Updates the custom drug name for custom-sourced records.
    /// </summary>
    /// <param name="customDrugName">New custom name.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateCustomDrugName(string customDrugName)
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

        if (string.Equals(CustomDrugName, normalizedName.Value, StringComparison.Ordinal))
        {
            return Result.Success;
        }

        CustomDrugName = normalizedName.Value;
        return Result.Success;
    }

    /// <summary>
    /// Attaches the catalog drug navigation after loading.
    /// </summary>
    /// <param name="drug">Catalog drug to attach.</param>
    public void AttachDrug(Drug drug)
    {
        Drug = drug;
        DrugId = drug.Id;
    }

    private static ErrorOr<(Guid UserId, Guid DrugId)> ValidateOwnership(Guid userId, Guid drugId)
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

        if (drugId == Guid.Empty)
        {
            return Error.Validation(
                code: "DrugProblem.DrugIdEmpty",
                description: "Drug id must be provided.",
                metadata: new()
                {
                    ["field"] = "drugId",
                });
        }

        return (userId, drugId);
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
}
