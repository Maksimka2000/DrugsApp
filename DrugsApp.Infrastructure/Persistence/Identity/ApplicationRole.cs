using System;
using Microsoft.AspNetCore.Identity;

namespace DrugsApp.Infrastructure.Persistence.Identity;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole()
    {
        Id = Guid.NewGuid();
        ConcurrencyStamp = Guid.NewGuid().ToString();
    }

    public ApplicationRole(string roleName) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
    }
}
