using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Persistence.Identity;

public sealed class ApplicationRoleStore : RoleStore<ApplicationRole, DrugsAppDbContext, Guid>
{
    public ApplicationRoleStore(DrugsAppDbContext context, IdentityErrorDescriber describer)
        : base(context, describer)
    {
        AutoSaveChanges = false;
    }
}
