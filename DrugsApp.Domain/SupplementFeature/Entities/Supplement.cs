using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.SupplementFeature.Entities;

/// <summary>
/// Represents a supplement with canonical name and aliases.
/// </summary>
public sealed class Supplement : BaseEntity
{
    /// <summary>
    /// Canonical display name of the supplement.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Normalized search text combining canonical name and aliases.
    /// </summary>
    public string SearchText { get; private set; } = string.Empty;

    private List<string> _aliases = new();

    /// <summary>
    /// Alternative names representing the same supplement concept.
    /// </summary>
    public IReadOnlyCollection<string> Aliases => _aliases;

    private Supplement()
    {
    }

    /// <summary>
    /// Creates a new supplement aggregate with canonical name and optional aliases.
    /// </summary>
    public static Supplement Create(string name, IReadOnlyCollection<string>? aliases)
    {
        string normalizedName = NormalizeName(name);

        var supplement = new Supplement
        {
            Name = normalizedName
        };

        supplement.ReplaceAliases(aliases);

        return supplement;
    }

    /// <summary>
    /// Updates the canonical name and refreshes search text.
    /// </summary>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        string normalized = NormalizeName(newName);
        if (normalized.Equals(Name, StringComparison.Ordinal))
        {
            return;
        }

        Name = normalized;
        ReplaceAliases(_aliases);
    }

    /// <summary>
    /// Replaces the entire alias collection with normalized values.
    /// </summary>
    public void ReplaceAliases(IEnumerable<string>? aliases)
    {
        List<string> cleanedAliases = NormalizeAliases(aliases, Name);

        _aliases = cleanedAliases;

        RebuildSearchText();
    }

    /// <summary>
    /// Builds the display-oriented search label: canonical name plus aliases in parentheses.
    /// </summary>
    public string BuildSearchOutputText()
    {
        if (_aliases.Count == 0)
        {
            return Name.ToLowerInvariant();
        }

        string aliases = string.Join(", ", _aliases.Select(alias => alias.ToLowerInvariant()));
        return $"{Name.ToLowerInvariant()} ({aliases})";
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
        return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
