using System;
using Microsoft.AspNetCore.Identity;

namespace DrugsApp.Infrastructure.Persistence.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Display name for admin user-management screens.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Account creation timestamp used for deterministic list ordering.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }
}
