using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.DrugProblemsFeature.Entities;

/// <summary>
/// Admin-managed dictionary entry representing a common drug problem note suggestion.
/// </summary>
public sealed class DrugProblemType : BaseEntity
{
    /// <summary>
    /// Display name of the drug problem type used in autocomplete and admin lists.
    /// </summary>
    public string ProblemTypeName { get; private set; } = null!;

    private DrugProblemType() : base()
    {
    }

    private DrugProblemType(string problemTypeName) : this()
    {
        ProblemTypeName = problemTypeName;
    }

    /// <summary>
    /// Creates a new drug problem type entry.
    /// </summary>
    /// <param name="problemTypeName">Problem type display name.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<DrugProblemType> Create(string problemTypeName)
    {
        ErrorOr<string> normalizedName = NormalizeName(problemTypeName);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        return new DrugProblemType(normalizedName.Value);
    }

    /// <summary>
    /// Updates the problem type name.
    /// </summary>
    /// <param name="problemTypeName">New name.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> UpdateName(string problemTypeName)
    {
        ErrorOr<string> normalizedName = NormalizeName(problemTypeName);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        if (string.Equals(ProblemTypeName, normalizedName.Value, StringComparison.Ordinal))
        {
            return Result.Success;
        }

        ProblemTypeName = normalizedName.Value;
        return Result.Success;
    }

    private static ErrorOr<string> NormalizeName(string value)
    {
        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: "DrugProblemType.NameEmpty",
                description: "Problem type name must be provided.",
                metadata: new()
                {
                    ["field"] = "problemTypeName",
                });
        }

        if (collapsed.Length > 200)
        {
            return Error.Validation(
                code: "DrugProblemType.NameTooLong",
                description: "Problem type name must not exceed 200 characters.",
                metadata: new()
                {
                    ["field"] = "problemTypeName",
                    ["maxLength"] = 200,
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
