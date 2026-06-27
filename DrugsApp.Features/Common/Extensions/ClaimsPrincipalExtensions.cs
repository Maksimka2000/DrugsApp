using System;
using System.Security.Claims;

namespace DrugsApp.Features.Common.Extensions;

/// <summary>
/// Extension helpers for extracting data from <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Attempts to parse the user id (subject) claim into a <see cref="Guid"/>.
    /// </summary>
    /// <param name="principal">Claims principal to read from.</param>
    /// <param name="userId">Parsed user id when available.</param>
    /// <returns><c>true</c> when the claim is present and parses to a Guid.</returns>
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        string? subject = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        return Guid.TryParse(subject, out userId);
    }
}
