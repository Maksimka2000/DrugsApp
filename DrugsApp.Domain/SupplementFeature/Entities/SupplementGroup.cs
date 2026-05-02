using System.Collections.ObjectModel;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.SupplementFeature.Entities;

public sealed class SupplementGroup : BaseEntity
{

    public string? Name { get; private set; } = null!;

    private readonly List<Supplement> _supplements = new();
    public IReadOnlyCollection<Supplement> Supplements => _supplements.AsReadOnly();

    private SupplementGroup()
    {
    }

    

 

    public static SupplementGroup Create(string name)
    {
        SupplementGroup group = new()
        {
            Name = NormalizeString(name)
        };
        return group;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        string normalized = NormalizeString(newName);
        if (normalized.Equals(Name, StringComparison.Ordinal)) return;
        Name = normalized;
        return;
    }

    public void UpdateSupplements(List<Supplement> supplements)
    {
        if (supplements is null || supplements.Count == 0)
        {
            return;
        }
        _supplements.Clear();
        _supplements.AddRange(supplements);
        return;
    }



    private static string NormalizeString(string value)
    {
        string trimmed = value.Trim();
        string collapsed = string.Join(
            ' ',
            trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return collapsed;
    }
}
