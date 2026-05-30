using System;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.SystemTextFeature.Entities;

/// <summary>
/// Represents a system-wide text snippet keyed by a stable identifier.
/// </summary>
public sealed class SystemText : BaseEntity
{
    /// <summary>
    /// Stable key used to reference the text.
    /// </summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Text value associated with the key.
    /// </summary>
    public string Value { get; private set; } = string.Empty;

    private SystemText() : base()
    {
    }

    private SystemText(string key, string value) : this()
    {
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Creates a new system text aggregate.
    /// </summary>
    /// <param name="key">Stable key.</param>
    /// <param name="value">Associated text content.</param>
    /// <returns>ErrorOr containing the aggregate or validation errors.</returns>
    public static ErrorOr<SystemText> Create(string key, string value)
    {
        string normalizedKey = NormalizeKey(key);
        if (string.IsNullOrWhiteSpace(normalizedKey))
        {
            return Error.Validation(
                code: "SystemText.KeyEmpty",
                description: "Key must be provided.",
                metadata: new()
                {
                    ["field"] = "key"
                });
        }

        string normalizedValue = NormalizeValue(value);
        if (normalizedValue.Length == 0)
        {
            return Error.Validation(
                code: "SystemText.ValueEmpty",
                description: "Value must be provided.",
                metadata: new()
                {
                    ["field"] = "value"
                });
        }

        return new SystemText(normalizedKey, normalizedValue);
    }

    /// <summary>
    /// Updates the stored text value.
    /// </summary>
    /// <param name="value">New text content.</param>
    /// <returns>ErrorOr indicating success or validation failure.</returns>
    public ErrorOr<Success> UpdateValue(string value)
    {
        string normalizedValue = NormalizeValue(value);
        if (normalizedValue.Length == 0)
        {
            return Error.Validation(
                code: "SystemText.ValueEmpty",
                description: "Value must be provided.",
                metadata: new()
                {
                    ["field"] = "value"
                });
        }

        Value = normalizedValue;
        return Result.Success;
    }

    private static string NormalizeKey(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NormalizeValue(string value)
    {
        string trimmed = value.Trim();
        return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
