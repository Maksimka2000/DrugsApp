using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.FoodFeature.Entities;

/// <summary>
/// Represents a single food item with canonical name and aliases.
/// </summary>
public sealed class Food : BaseEntity
{
    

    private Food() : base()
    {
    }

    /// <summary>
    /// Canonical display name of the food.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Normalized search text combining canonical name and aliases.
    /// </summary>
    public string SearchText { get; private set; } = string.Empty;

    private readonly List<string> _aliases = new();
    /// <summary>
    /// Alternative names representing the same food concept.
    /// </summary>
    /// 
    public IReadOnlyCollection<string> Aliases => _aliases;

    /// <summary>
    /// Creates a new food aggregate with canonical name and optional aliases.
    /// </summary>
    /// <param name="name">Canonical food name.</param>
    /// <param name="aliases">Optional alias list.</param>
    /// <returns>New <see cref="Food"/> or validation errors.</returns>
    public static ErrorOr<Food> Create(
        string name,
        IEnumerable<string>? aliases)
    {
        string normalizedName = NormalizeName(name);
        if (string.IsNullOrEmpty(normalizedName))
        {
            return Error.Validation(
                description: "Food name cannot be empty.",
                metadata: new Dictionary<string, object>
                {
                    ["error"] = "Food name is required."
                });
        }

        List<string> cleanedAliases = NormalizeAliases(aliases, normalizedName);

        var food = new Food
        {
            Name = normalizedName
        };

        food._aliases.AddRange(cleanedAliases);
        food.RebuildSearchText();

        return food;
    }

    /// <summary>
    /// Updates the canonical name and refreshes search text.
    /// </summary>
    /// <param name="name">New canonical name.</param>
    /// <returns>Success or validation errors.</returns>
    public void UpdateName(string name)
    {
        Name = name;

        ReplaceAliases(_aliases);
    }

    /// <summary>
    /// Replaces the entire alias collection with normalized values.
    /// </summary>
    /// <param name="aliases">New alias list.</param>
    /// <returns>Success result.</returns>
    public void ReplaceAliases(IEnumerable<string>? aliases)
    {
        List<string> cleanedAliases = NormalizeAliases(aliases, Name);

        _aliases.Clear();
        _aliases.AddRange(cleanedAliases);

        RebuildSearchText();
        return;
    }

    private void RebuildSearchText()
    {
        IEnumerable<string> parts = new[] { Name }.Concat(_aliases);
        SearchText = string.Join(' ', parts)
            .Trim()
            .ToLowerInvariant();
    }

    private static List<string> NormalizeAliases(IEnumerable<string>? aliases, string canonicalName)
    {
        string normalizedName = canonicalName.ToLowerInvariant();
        var cleaned = new List<string>();
        if (aliases is null)
        {
            return cleaned;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string alias in aliases)
        {
            string normalizedAlias = NormalizeName(alias);
            if (string.IsNullOrEmpty(normalizedAlias))
            {
                continue;
            }

            if (normalizedAlias.Equals(normalizedName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (seen.Add(normalizedAlias))
            {
                cleaned.Add(normalizedAlias);
            }
        }

        return cleaned;
    }

     private static string NormalizeName(string value)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return collapsed;
    }


}
