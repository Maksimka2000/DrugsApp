using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.FoodFeature.Entities;

/// <summary>
/// Represents a named collection of foods managed as a group.
/// </summary>
public sealed class FoodGroup : BaseEntity
{
    private readonly List<Food> _foods = new();

    private FoodGroup() : base()
    {
    }

    /// <summary>
    /// Display name of the food group.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Foods belonging to this group.
    /// </summary>
    public IReadOnlyCollection<Food> Foods => _foods;

    /// <summary>
    /// Creates a new food group with the provided members.
    /// </summary>
    /// <param name="name">Group name.</param>
    /// <param name="foods">Initial member foods.</param>
    /// <returns>New <see cref="FoodGroup"/> or validation errors.</returns>
    public static ErrorOr<FoodGroup> Create(
        string name,
        IEnumerable<Food> foods)
    {
        string normalizedName = NormalizeName(name);
        if (string.IsNullOrEmpty(normalizedName))
        {
            return Error.Validation(
                description: "Food group name cannot be empty.",
                metadata: new Dictionary<string, object>
                {
                    ["error"] = "Food group name is required."
                });
        }

        List<Food> distinctFoods = NormalizeMembers(foods);
        if (distinctFoods.Count == 0)
        {
            return Error.Validation(
                description: "Food group requires at least one food.",
                metadata: new Dictionary<string, object>
                {
                    ["error"] = "Food group members are required."
                });
        }

        var group = new FoodGroup
        {
            Name = normalizedName
        };

        group.ReplaceMembers(distinctFoods);

        return group;
    }

    /// <summary>
    /// Updates the group name.
    /// </summary>
    /// <param name="name">New name.</param>
    /// <returns>Success or validation errors.</returns>
    public void UpdateName(string name)
    {
        if (Name.Equals(name, StringComparison.Ordinal))
        {
            return;
        }

        Name = name;
    }

    /// <summary>
    /// Replaces the entire membership set with the provided foods.
    /// </summary>
    /// <param name="foods">New member foods.</param>
    /// <returns>Success or validation errors.</returns>
    public void ReplaceMembers(IEnumerable<Food> foods)
    {
        List<Food> distinctFoods = NormalizeMembers(foods);
 
        _foods.Clear();
        _foods.AddRange(distinctFoods);

        return;
    }

    private static List<Food> NormalizeMembers(IEnumerable<Food> foods)
    {
        return foods
            .Where(food => food is not null)
            .DistinctBy(food => food.Id)
            .ToList();
    }

    private static string NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string trimmed = value.Trim();
        string collapsed = string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return collapsed;
    }
}
