using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DrugsApp.Infrastructure.Persistence.Identity;

public sealed class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, DrugsAppDbContext, Guid>
{
    public ApplicationUserStore(DrugsAppDbContext context, IdentityErrorDescriber describer)
        : base(context, describer)
    {
        AutoSaveChanges = false;
    }
}
