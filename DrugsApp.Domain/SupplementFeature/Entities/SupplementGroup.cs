using System.Collections.ObjectModel;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.SupplementFeature.Entities;

/// <summary>
/// Represents a group of supplements, providing functionality to manage its name and associated supplements.
/// </summary>
public sealed class SupplementGroup : BaseEntity
{
    /// <summary>
    /// Gets the name of the supplement group.
    /// </summary>
    /// <remarks>
    /// This property holds the normalized name of the <see cref="SupplementGroup"/>. It is a nullable string
    /// that is initialized during the creation of the group and can be updated using the appropriate method.
    /// The value ensures case-insensitivity and consistent formatting through normalization, enhancing user experience
    /// and data integrity.
    /// </remarks>
    public string? Name { get; private set; } = null!;

    private readonly List<Supplement> _supplements = new();
    /// <summary>
    /// Represents the collection of supplements belonging to the supplement group.
    /// </summary>
    /// <remarks>
    /// This property provides a read-only collection of <see cref="Supplement"/> entities
    /// associated with the <see cref="SupplementGroup"/>. It ensures the encapsulation of the underlying
    /// list and prevents direct modifications to the collection from outside the
    /// <see cref="SupplementGroup"/> class.
    /// </remarks>
    public IReadOnlyCollection<Supplement> Supplements => _supplements.AsReadOnly();

    private SupplementGroup()
    {
    }


    /// <summary>
    /// Creates a new instance of the SupplementGroup entity with the specified name.
    /// Initializes the Name property after normalizing the provided string.
    /// </summary>
    /// <param name="name">
    /// The name of the supplement group to create.
    /// It will be normalized to collapse extra whitespace and trim leading or trailing spaces.
    /// </param>
    /// <returns>
    /// A new instance of the SupplementGroup entity with the specified name.
    /// </returns>
    public static SupplementGroup Create(string name)
    {
        SupplementGroup group = new()
        {
            Name = NormalizeString(name)
        };
        return group;
    }

    /// <summary>
    /// Updates the name of the supplement group.
    /// Replaces the current name with the provided name, if it is valid and different from the current name.
    /// </summary>
    /// <param name="newName">
    /// The new name to set for the supplement group.
    /// If the provided name is null, empty, or identical (case-insensitive and whitespace-normalized) to the current name, no changes are made.
    /// </param>
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return;
        string normalized = NormalizeString(newName);
        if (normalized.Equals(Name, StringComparison.Ordinal)) return;
        Name = normalized;
        return;
    }

    /// <summary>
    /// Updates the supplements associated with the supplement group.
    /// Replaces the current collection of supplements with the provided list of supplements.
    /// </summary>
    /// <param name="supplements">
    /// A list of supplements to associate with this supplement group.
    /// If the provided list is null or empty, the current collection remains unchanged.
    /// </param>
    public void UpdateSupplements(List<Supplement>? supplements)
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
