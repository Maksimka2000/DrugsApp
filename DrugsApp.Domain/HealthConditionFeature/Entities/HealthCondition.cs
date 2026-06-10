using System;
using System.Collections.Generic;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.HealthConditionFeature.Entities;

/// <summary>
/// Catalog health condition entry shared between admin and client applications.
/// </summary>
public sealed class HealthCondition : BaseEntity
{
    // Relationships
    private readonly List<UserHealthCondition> _userHealthConditions = new();

    /// <summary>
    /// User health conditions linked to this catalog entry.
    /// </summary>
    public IReadOnlyCollection<UserHealthCondition> UserHealthConditions => _userHealthConditions;

    // Attributes

    /// <summary>
    /// Display name of the health condition.
    /// </summary>
    public string HealthConditionName { get; private set; } = string.Empty;

    /// <summary>
    /// Lower-case collapsed lookup value used for uniqueness and ordering.
    /// </summary>
    public string NormalizedHealthConditionName { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description managed by admins; stored as null when blank.
    /// </summary>
    public string? Description { get; private set; }

    private HealthCondition() : base()
    {
    }

    private HealthCondition(
        string healthConditionName,
        string normalizedHealthConditionName,
        string? description) : this()
    {
        HealthConditionName = healthConditionName;
        NormalizedHealthConditionName = normalizedHealthConditionName;
        Description = description;
    }

    /// <summary>
    /// Creates a validated health condition aggregate.
    /// </summary>
    /// <param name="healthConditionName">Catalog health condition name.</param>
    /// <param name="description">Optional description.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<HealthCondition> Create(string healthConditionName, string? description)
    {
        ErrorOr<(string DisplayValue, string LookupValue)> normalizedName = NormalizeName(healthConditionName);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<string?> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsError)
        {
            return normalizedDescription.Errors;
        }

        return new HealthCondition(
            healthConditionName: normalizedName.Value.DisplayValue,
            normalizedHealthConditionName: normalizedName.Value.LookupValue,
            description: normalizedDescription.Value);
    }

    /// <summary>
    /// Updates the catalog health condition using normalized values when provided.
    /// </summary>
    /// <param name="healthConditionName">Optional new name; null means no change.</param>
    /// <param name="description">Optional new description; null means no change.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> Update(string? healthConditionName, string? description)
    {
        if (!string.IsNullOrWhiteSpace(healthConditionName))
        {
            ErrorOr<(string DisplayValue, string LookupValue)> normalizedName = NormalizeName(healthConditionName);
            if (normalizedName.IsError)
            {
                return normalizedName.Errors;
            }

            HealthConditionName = normalizedName.Value.DisplayValue;
            NormalizedHealthConditionName = normalizedName.Value.LookupValue;
        }

        if (description is not null)
        {
            ErrorOr<string?> normalizedDescription = NormalizeDescription(description);
            if (normalizedDescription.IsError)
            {
                return normalizedDescription.Errors;
            }

            Description = normalizedDescription.Value;
        }

        return Result.Success;
    }

    private static ErrorOr<(string DisplayValue, string LookupValue)> NormalizeName(string value)
    {
        string collapsed = CollapseWhitespace(value);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                code: "HealthCondition.NameEmpty",
                description: "Health condition name must be provided.",
                metadata: new()
                {
                    ["field"] = "healthConditionName"
                });
        }

        if (collapsed.Length > 256)
        {
            return Error.Validation(
                code: "HealthCondition.NameTooLong",
                description: "Health condition name must not exceed 256 characters.",
                metadata: new()
                {
                    ["field"] = "healthConditionName",
                    ["maxLength"] = 256
                });
        }

        return (collapsed, collapsed.ToLowerInvariant());
    }

    private static ErrorOr<string?> NormalizeDescription(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (string?)null;
        }

        string trimmed = value.Trim();
        if (trimmed.Length > 2000)
        {
            return Error.Validation(
                code: "HealthCondition.DescriptionTooLong",
                description: "Description must not exceed 2000 characters.",
                metadata: new()
                {
                    ["field"] = "description",
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
