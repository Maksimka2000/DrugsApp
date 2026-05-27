using System.Collections.ObjectModel;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.SupplementFeature.Entities;

/// <summary>
/// 
/// </summary>
public sealed class Supplement : BaseEntity
{
    /// <summary>
    /// Gets the name of the supplement.
    /// This value is normalized when set and cannot be null, empty, or whitespace.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Gets the concatenated search text used for searching the supplement.
    /// This value is composed of the supplement's name and its associated aliases,
    /// recalculated whenever these properties are updated.
    /// </summary>
    public string SearchText { get; private set; } = string.Empty;

    // list of aliases
    private readonly List<string> _aliases = new();

    /// <summary>
    /// Gets the collection of aliases associated with the supplement.
    /// This collection is read-only and reflects any alternative names
    /// or identifiers for the supplement.
    /// </summary>
    public IReadOnlyCollection<string> Aliases => _aliases.AsReadOnly();

    // navigation properties
    private Supplement()
    {
    }


    /// <summary>
    /// Creates a new instance of the <see cref="Supplement"/> class by normalizing the name
    /// and optionally setting aliases. If aliases are provided, they will be processed and stored.
    /// </summary>
    /// <param name="name">The name of the supplement. This value will be normalized
    /// before being set. It cannot be null, empty, or whitespace.</param>
    /// <param name="aliases">An optional collection of aliases for the supplement.
    /// If provided, these will be normalized and set. If null or empty, no aliases are added.</param>
    /// <returns>A new instance of the <see cref="Supplement"/> class with the specified name
    /// and optional aliases.</returns>
    public static Supplement Create(string name, IReadOnlyCollection<string>? aliases)
    {
        string normalizedName = NormalizeString(name);

        Supplement supplement = new()
        {
            Name = normalizedName
        };

        if (aliases is not null && aliases.Count > 0)
        {
            supplement.UpdateAliases(aliases);
        }
 
        return supplement;
    }




    // methods 
    /// <summary>
    /// Updates the name of the supplement by normalizing the provided value and replacing the current name.
    /// This method also recalculates the search text for the supplement.
    /// </summary>
    /// <param name="newName">The new name to replace the current name.
    /// The value will be normalized before being set. If null, empty, or whitespace, the name is not updated.</param>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        string normalized = NormalizeString(newName);
        if (normalized.Equals(Name, StringComparison.Ordinal)) return;
        Name = normalized;
        RecalculateSearchText();
        return;
    }

    /// <summary>
    /// Updates the list of aliases for the supplement by normalizing and replacing the existing aliases.
    /// This method also recalculates the search text for the supplement.
    /// </summary>
    /// <param name="aliases">A collection of new aliases to replace the current list.
    /// The aliases will be normalized before being set. If null or empty, the current aliases are not modified.</param>
    public void UpdateAliases(IReadOnlyCollection<string> aliases)
    {
        if (aliases is null || aliases.Count == 0)
        {
            return;
        }
        List<string> cleanedAliases = NormalizeStrings(aliases);
        _aliases.Clear();
        _aliases.AddRange(cleanedAliases);
        RecalculateSearchText();
        return;
    }



    // helper methods
    private void RecalculateSearchText()
    {
        if (_aliases.Count == 0)
        {
            SearchText = Name;
            return;
        }

        SearchText = string.Join(' ', new[] { Name }.Concat(_aliases));
    }

    private static string NormalizeString(string value)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(
            ' ',
            trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return collapsed;
    }

    private static List<string> NormalizeStrings(IReadOnlyCollection<string> values) => values.Select(NormalizeString).Where(value => true).ToList();
}
