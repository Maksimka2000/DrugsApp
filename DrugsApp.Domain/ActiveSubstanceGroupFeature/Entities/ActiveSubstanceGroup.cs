using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Domain.Common;

namespace DrugsApp.Domain.ActiveSubstanceGroupFeature.Entities;

/// <summary>
/// Represents a reusable group of active substances for interaction configuration.
/// </summary>
public sealed class ActiveSubstanceGroup : BaseEntity
{
    private readonly List<ActiveSubstance> _activeSubstances = new();

    private ActiveSubstanceGroup() : base()
    {
    }

    /// <summary>
    /// Display name of the active substance group.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Members belonging to this group.
    /// </summary>
    public IReadOnlyCollection<ActiveSubstance> ActiveSubstances => _activeSubstances;

    /// <summary>
    /// Creates a new active substance group with the provided members.
    /// </summary>
    /// <param name="name">Group name.</param>
    /// <param name="members">Initial members to attach.</param>
    /// <returns>New <see cref="ActiveSubstanceGroup"/> instance.</returns>
    public static ActiveSubstanceGroup Create(
        string name,
        IEnumerable<ActiveSubstance> members)
    {
        var group = new ActiveSubstanceGroup
        {
            Name = name.Trim()
        };

        group.ReplaceMembers(members);

        return group;
    }

    /// <summary>
    /// Updates the group name when it differs from the current value.
    /// </summary>
    /// <param name="name">New group name.</param>
    public void UpdateName(string name)
    {
        string trimmedName = name.Trim();

        if (Name == trimmedName)
        {
            return;
        }

        Name = trimmedName;
    }

    /// <summary>
    /// Replaces the entire membership set with the provided active substances.
    /// </summary>
    /// <param name="members">New member collection.</param>
    public void ReplaceMembers(IEnumerable<ActiveSubstance> members)
    {
        _activeSubstances.Clear();

        foreach (ActiveSubstance member in members.DistinctBy(member => member.Id))
        {
            _activeSubstances.Add(member);
        }
    }
}
