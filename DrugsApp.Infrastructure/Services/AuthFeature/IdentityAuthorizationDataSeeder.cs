using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Enums;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

public sealed class IdentityAuthorizationDataSeeder(
    RoleManager<ApplicationRole> _roleManager,
    UserManager<ApplicationUser> _userManager,
    DrugsAppDbContext _dbContext,
    IOptions<SuperAdminSettings> superAdminOptions)
{
    private readonly SuperAdminSettings _superAdminSettings = superAdminOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Identity stores use AutoSaveChanges = false. Each role must be persisted before
        // permission claims are added (FK on AspNetRoleClaims.RoleId) and before AddToRoleAsync
        // looks up roles by normalized name (e.g. ADMIN).
        foreach (string roleName in AuthRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await EnsureRoleAsync(roleName);
            await _dbContext.SaveChangesAsync(cancellationToken);

            ApplicationRole role = (await _roleManager.FindByNameAsync(roleName))!;
            await SynchronizePermissionClaimsAsync(
                role,
                PermissionCatalog.RolePermissions[roleName],
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        await EnsureSuperAdminAsync(cancellationToken);
    }

    private async Task EnsureSuperAdminAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_superAdminSettings.Email) ||
            string.IsNullOrWhiteSpace(_superAdminSettings.Password))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();

        string email = _superAdminSettings.Email.Trim();
        ApplicationUser? user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                FullName = "Super Admin",
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult createResult = await _userManager.CreateAsync(user, _superAdminSettings.Password);
            EnsureSucceeded(createResult, "Failed to create super admin user.");
            await _dbContext.SaveChangesAsync(cancellationToken);
            user = (await _userManager.FindByEmailAsync(email))!;
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            IdentityResult updateResult = await _userManager.UpdateAsync(user);
            EnsureSucceeded(updateResult, "Failed to confirm super admin email.");
            await _dbContext.SaveChangesAsync(cancellationToken);
            user = (await _userManager.FindByEmailAsync(email))!;
        }

        ApplicationRole adminRole = (await _roleManager.FindByNameAsync(AuthRoles.Admin))!;

        bool hasAdminRole = await _dbContext.UserRoles.AnyAsync(
            userRole => userRole.UserId == user.Id && userRole.RoleId == adminRole.Id,
            cancellationToken);

        if (!hasAdminRole)
        {
            _dbContext.UserRoles.Add(new IdentityUserRole<Guid>
            {
                UserId = user.Id,
                RoleId = adminRole.Id
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<ApplicationRole> EnsureRoleAsync(string roleName)
    {
        ApplicationRole? existingRole = await _roleManager.FindByNameAsync(roleName);
        if (existingRole is not null)
        {
            return existingRole;
        }

        var role = new ApplicationRole(roleName);
        IdentityResult createResult = await _roleManager.CreateAsync(role);
        EnsureSucceeded(createResult, $"Failed to create role '{roleName}'.");

        return role;
    }

    private async Task SynchronizePermissionClaimsAsync(
        ApplicationRole role,
        IReadOnlyCollection<Permission> targetPermissions,
        CancellationToken cancellationToken)
    {
        // Write claims directly to avoid RoleManager updating AspNetRoles.ConcurrencyStamp,
        // which conflicts with AutoSaveChanges = false on the Identity stores.
        List<IdentityRoleClaim<Guid>> existingClaims = await _dbContext.RoleClaims
            .Where(claim => claim.RoleId == role.Id && claim.ClaimType == PermissionClaimTypes.Permission)
            .ToListAsync(cancellationToken);

        var targetPermissionNames = targetPermissions
            .Select(permission => permission.ToString())
            .ToHashSet(StringComparer.Ordinal);

        foreach (IdentityRoleClaim<Guid> existingClaim in existingClaims)
        {
            if (targetPermissionNames.Contains(existingClaim.ClaimValue))
            {
                continue;
            }

            _dbContext.RoleClaims.Remove(existingClaim);
        }

        HashSet<string> existingPermissionNames = existingClaims
            .Where(claim => targetPermissionNames.Contains(claim.ClaimValue))
            .Select(claim => claim.ClaimValue)
            .ToHashSet(StringComparer.Ordinal);

        foreach (string targetPermissionName in targetPermissionNames)
        {
            if (existingPermissionNames.Contains(targetPermissionName))
            {
                continue;
            }

            _dbContext.RoleClaims.Add(new IdentityRoleClaim<Guid>
            {
                RoleId = role.Id,
                ClaimType = PermissionClaimTypes.Permission,
                ClaimValue = targetPermissionName
            });
        }
    }

    private static void EnsureSucceeded(IdentityResult identityResult, string message)
    {
        if (identityResult.Succeeded)
        {
            return;
        }

        string detail = string.Join("; ", identityResult.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {detail}");
    }
}
