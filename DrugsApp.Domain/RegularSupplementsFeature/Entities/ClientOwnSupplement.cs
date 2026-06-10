using System;
using System.Collections.Generic;
using System.Linq;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.RegularSupplementsFeature.Entities;

/// <summary>
/// User-private compound supplement with a display name and ordered catalog members.
/// </summary>
public sealed class ClientOwnSupplement : BaseEntity
{
    private const int MaxNameLength = 200;

    // Attributes

    /// <summary>
    /// Authenticated owner identifier scoped to every query and command.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Trimmed display name unique case-insensitively per user.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Lowercase trimmed name used for uniqueness queries.
    /// </summary>
    public string NormalizedName { get; private set; } = null!;

    // Relationships

    private readonly List<ClientOwnSupplementItem> _items = new();

    /// <summary>
    /// Ordered catalog supplement members owned by this compound.
    /// </summary>
    public IReadOnlyCollection<ClientOwnSupplementItem> Items => _items.AsReadOnly();

    private ClientOwnSupplement() : base()
    {
    }

    private ClientOwnSupplement(Guid userId, string name, string normalizedName) : this()
    {
        UserId = userId;
        Name = name;
        NormalizedName = normalizedName;
    }

    /// <summary>
    /// Creates a new own supplement compound for the specified user.
    /// </summary>
    /// <param name="userId">Owner identifier.</param>
    /// <param name="name">Display name for the compound.</param>
    /// <param name="memberSupplementIds">Distinct catalog supplement identifiers in display order.</param>
    /// <returns>Validated aggregate or validation errors.</returns>
    public static ErrorOr<ClientOwnSupplement> Create(
        Guid userId,
        string name,
        IReadOnlyCollection<Guid> memberSupplementIds)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                description: "User identifier is required.",
                metadata: new()
                {
                    ["field"] = "userId",
                });
        }

        ErrorOr<(string Display, string Normalized)> normalizedName = NormalizeName(name);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<List<Guid>> normalizedMembers = NormalizeMemberSupplementIds(memberSupplementIds);
        if (normalizedMembers.IsError)
        {
            return normalizedMembers.Errors;
        }

        ClientOwnSupplement aggregate = new(
            userId,
            normalizedName.Value.Display,
            normalizedName.Value.Normalized);

        ErrorOr<Success> membersResult = aggregate.ReplaceMembersInternal(normalizedMembers.Value);
        if (membersResult.IsError)
        {
            return membersResult.Errors;
        }

        return aggregate;
    }

    /// <summary>
    /// Replaces the display name and member set on an existing own supplement.
    /// </summary>
    /// <param name="name">New display name.</param>
    /// <param name="memberSupplementIds">Distinct catalog supplement identifiers in display order.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> Update(string name, IReadOnlyCollection<Guid> memberSupplementIds)
    {
        ErrorOr<(string Display, string Normalized)> normalizedName = NormalizeName(name);
        if (normalizedName.IsError)
        {
            return normalizedName.Errors;
        }

        ErrorOr<List<Guid>> normalizedMembers = NormalizeMemberSupplementIds(memberSupplementIds);
        if (normalizedMembers.IsError)
        {
            return normalizedMembers.Errors;
        }

        Name = normalizedName.Value.Display;
        NormalizedName = normalizedName.Value.Normalized;

        return ReplaceMembersInternal(normalizedMembers.Value);
    }

    /// <summary>
    /// Normalizes a display name for case-insensitive uniqueness checks.
    /// </summary>
    /// <param name="name">Raw display name.</param>
    /// <returns>Trimmed display and lowercase lookup values or validation errors.</returns>
    public static ErrorOr<(string Display, string Normalized)> NormalizeName(string name)
    {
        string collapsed = CollapseWhitespace(name);
        if (string.IsNullOrWhiteSpace(collapsed))
        {
            return Error.Validation(
                description: "Name must not be empty.",
                metadata: new()
                {
                    ["field"] = "name",
                });
        }

        if (collapsed.Length > MaxNameLength)
        {
            return Error.Validation(
                description: $"Name must not exceed {MaxNameLength} characters.",
                metadata: new()
                {
                    ["field"] = "name",
                    ["maxLength"] = MaxNameLength,
                });
        }

        return (collapsed, collapsed.ToLowerInvariant());
    }

    private ErrorOr<Success> ReplaceMembersInternal(List<Guid> memberSupplementIds)
    {
        _items.Clear();

        for (int index = 0; index < memberSupplementIds.Count; index++)
        {
            ErrorOr<ClientOwnSupplementItem> child = ClientOwnSupplementItem.Create(memberSupplementIds[index], index);
            if (child.IsError)
            {
                return child.Errors;
            }

            child.Value.AssignOwnSupplementId(Id);
            _items.Add(child.Value);
        }

        return Result.Success;
    }

    private static ErrorOr<List<Guid>> NormalizeMemberSupplementIds(IReadOnlyCollection<Guid>? memberSupplementIds)
    {
        List<Guid> normalizedIds = memberSupplementIds?.ToList() ?? new List<Guid>();

        if (normalizedIds.Count == 0)
        {
            return Error.Validation(
                description: "At least one supplement member is required.",
                metadata: new()
                {
                    ["field"] = "memberSupplementIds",
                });
        }

        if (normalizedIds.Any(id => id == Guid.Empty))
        {
            return Error.Validation(
                description: "Supplement identifier is required.",
                metadata: new()
                {
                    ["field"] = "memberSupplementIds",
                });
        }

        if (normalizedIds.Count != normalizedIds.Distinct().Count())
        {
            return Error.Validation(
                description: "Supplement members must be unique.",
                metadata: new()
                {
                    ["field"] = "memberSupplementIds",
                });
        }

        return normalizedIds;
    }

    private static string CollapseWhitespace(string value)
    {
        string trimmed = value.Trim();
        return string.Join(
            ' ',
            trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
