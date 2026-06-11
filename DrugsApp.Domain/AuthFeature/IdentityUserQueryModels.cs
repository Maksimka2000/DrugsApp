using System;
using System.Collections.Generic;

namespace DrugsApp.Domain.AuthFeature;

/// <summary>
/// Identity user row projected for admin user listing and detail reads.
/// </summary>
public sealed record IdentityAdminUserRow(
    Guid Id,
    string FullName,
    string Email,
    string AdminRole,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    DateTime CreatedAtUtc);

/// <summary>
/// Identity user row projected for client user listing reads.
/// </summary>
public sealed record IdentityClientUserRow(
    Guid Id,
    string Email,
    IReadOnlyList<string> Roles,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    bool EmailConfirmed,
    DateTime CreatedAtUtc);

/// <summary>
/// Paginated admin identity user search result.
/// </summary>
public sealed record IdentityAdminUserSearchResult(
    IReadOnlyList<IdentityAdminUserRow> Items,
    int TotalCount);

/// <summary>
/// Paginated client identity user search result.
/// </summary>
public sealed record IdentityClientUserSearchResult(
    IReadOnlyList<IdentityClientUserRow> Items,
    int TotalCount);
