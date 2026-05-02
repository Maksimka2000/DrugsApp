using System.Collections.ObjectModel;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.SupplementFeature.Entities;

/// <summary>
/// 
/// </summary>
public sealed class Supplement : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string SearchText { get; private set; } = string.Empty;

    // list of aliases
    private readonly List<string> _aliases = new();
    public IReadOnlyCollection<string> Aliases => _aliases.AsReadOnly();

    // navigation properties
    private Supplement()
    {
    }

    



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
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        string normalized = NormalizeString(newName);
        if (normalized.Equals(Name, StringComparison.Ordinal)) return;
        Name = normalized;
        RecalculateSearchText();
        return;
    }

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

    private static List<string> NormalizeStrings(IReadOnlyCollection<string> values)
    {
        return values.Select(NormalizeString).Where(value => value is not null).ToList();
    }
}
