using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Persistence.Identity;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

/// <summary>
/// Identity mutations for admin and client user management workflows.
/// </summary>
public sealed class IdentityUserManagementService(
    UserManager<ApplicationUser> _userManager,
    RoleManager<ApplicationRole> _roleManager,
    DrugsAppDbContext _dbContext,
    IRefreshTokenRepository _refreshTokenRepository,
    IUserPermissionService _userPermissionService,
    ISuperAdminGuard _superAdminGuard) : IIdentityUserManagementService
{
    /// <inheritdoc />
    public async Task<ErrorOr<CreateAdminUserResult>> CreateAdminUserAsync(
        string fullName,
        string email,
        string adminRole,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        string trimmedFullName = fullName.Trim();

        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return Error.Conflict(
                code: "Auth.EmailAlreadyExists",
                description: "An account with this email already exists.",
                metadata: new Dictionary<string, object> { ["field"] = "email" });
        }

        string generatedPassword = GenerateSecurePassword();

        var user = new ApplicationUser
        {
            Email = normalizedEmail,
            UserName = normalizedEmail,
            EmailConfirmed = true,
            FullName = trimmedFullName,
            CreatedAtUtc = DateTime.UtcNow,
            LockoutEnabled = true,
            LockoutEnd = null
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, generatedPassword);
        if (!createResult.Succeeded)
        {
            return MapIdentityFailure(createResult);
        }

        await EnsureNewIdentityUserPersistedAsync(user, cancellationToken);
        user = (await _userManager.FindByIdAsync(user.Id.ToString()))!;

        ErrorOr<Success> assignRoleResult = await AddUserToRoleDirectAsync(
            user.Id,
            adminRole,
            cancellationToken);
        if (assignRoleResult.IsError)
        {
            return assignRoleResult.Errors;
        }

        return new CreateAdminUserResult(
            Id: user.Id,
            FullName: trimmedFullName,
            Email: normalizedEmail,
            AdminRole: adminRole,
            Password: generatedPassword);
    }

    /// <inheritdoc />
    public async Task<ErrorOr<AdminUserMutationResult>> ReplaceAdminRoleAsync(
        Guid adminUserId,
        string adminRole,
        Guid callerUserId,
        CancellationToken cancellationToken)
    {
        if (adminUserId == callerUserId)
        {
            return Error.Forbidden(
                code: "Auth.SelfRoleChangeForbidden",
                description: "You cannot change your own admin role.");
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (user is null)
        {
            return AdminUserNotFound();
        }

        IList<string> roles = await _userManager.GetRolesAsync(user);
        if (!AuthPortalClassifier.IsAdminPortalUser(roles))
        {
            return AdminUserNotFound();
        }

        ErrorOr<Success> mutableResult = await _superAdminGuard.EnsureMutableAsync(user.Email!, cancellationToken);
        if (mutableResult.IsError)
        {
            return mutableResult.Errors;
        }

        foreach (string existingAdminRole in roles.Where(AuthRoles.AdminRoleNames.Contains))
        {
            IdentityResult removeResult = await _userManager.RemoveFromRoleAsync(user, existingAdminRole);
            if (!removeResult.Succeeded)
            {
                return MapIdentityFailure(removeResult);
            }
        }

        ErrorOr<Success> assignRoleResult = await AddUserToRoleDirectAsync(user.Id, adminRole, cancellationToken);
        if (assignRoleResult.IsError)
        {
            return assignRoleResult.Errors;
        }

        await _userPermissionService.InvalidateAsync(user.Id, cancellationToken);
        await _refreshTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);

        return await BuildAdminMutationResultAsync(user, adminRole, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> SetAdminPasswordAsync(
        Guid adminUserId,
        string newPassword,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (user is null || !await IsAdminPortalUserAsync(user))
        {
            return AdminUserNotFound();
        }

        if (await _userManager.HasPasswordAsync(user))
        {
            IdentityResult removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                return MapIdentityFailure(removeResult);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        IdentityResult addResult = await _userManager.AddPasswordAsync(user, newPassword);
        if (!addResult.Succeeded)
        {
            return MapPasswordFailure(addResult);
        }

        await _refreshTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);

        return Result.Success;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<AdminUserMutationResult>> ChangeAdminLoginEmailAsync(
        Guid adminUserId,
        string email,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (user is null || !await IsAdminPortalUserAsync(user))
        {
            return AdminUserNotFound();
        }

        ErrorOr<Success> mutableResult = await _superAdminGuard.EnsureMutableAsync(user.Email!, cancellationToken);
        if (mutableResult.IsError)
        {
            return mutableResult.Errors;
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();
        ApplicationUser? conflictUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (conflictUser is not null && conflictUser.Id != user.Id)
        {
            return Error.Conflict(
                code: "Auth.EmailAlreadyExists",
                description: "An account with this email already exists.",
                metadata: new Dictionary<string, object> { ["field"] = "email" });
        }

        IdentityResult emailResult = await _userManager.SetEmailAsync(user, normalizedEmail);
        if (!emailResult.Succeeded)
        {
            return MapIdentityFailure(emailResult);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        IdentityResult usernameResult = await _userManager.SetUserNameAsync(user, normalizedEmail);
        if (!usernameResult.Succeeded)
        {
            return MapIdentityFailure(usernameResult);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        user.EmailConfirmed = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _refreshTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);

        string adminRole = await GetPrimaryAdminRoleAsync(user.Id, cancellationToken);
        return await BuildAdminMutationResultAsync(user, adminRole, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ErrorOr<Success>> RemoveAdminUserAsync(
        Guid adminUserId,
        Guid callerUserId,
        CancellationToken cancellationToken)
    {
        if (adminUserId == callerUserId)
        {
            return Error.Forbidden(
                code: "Auth.SelfRemovalForbidden",
                description: "You cannot remove your own admin account.");
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (user is null || !await IsAdminPortalUserAsync(user))
        {
            return AdminUserNotFound();
        }

        if (_superAdminGuard.IsProtectedEmail(user.Email!))
        {
            return Error.Forbidden(
                code: "Auth.ProtectedAccount",
                description: "This account is protected and cannot be removed.");
        }

        await _refreshTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
        await _userPermissionService.InvalidateAsync(user.Id, cancellationToken);

        IdentityResult deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            return MapIdentityFailure(deleteResult);
        }

        return Result.Success;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<UserSuspensionResult>> SetAdminSuspensionAsync(
        Guid adminUserId,
        bool isSuspended,
        Guid callerUserId,
        CancellationToken cancellationToken)
    {
        if (isSuspended && adminUserId == callerUserId)
        {
            return Error.Forbidden(
                code: "Auth.SelfSuspensionForbidden",
                description: "You cannot suspend your own admin account.");
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(adminUserId.ToString());
        if (user is null || !await IsAdminPortalUserAsync(user))
        {
            return AdminUserNotFound();
        }

        ErrorOr<Success> mutableResult = await _superAdminGuard.EnsureMutableAsync(user.Email!, cancellationToken);
        if (mutableResult.IsError)
        {
            return mutableResult.Errors;
        }

        ErrorOr<UserSuspensionResult> suspensionResult = await ApplySuspensionAsync(user, isSuspended, cancellationToken);
        if (suspensionResult.IsError)
        {
            return suspensionResult.Errors;
        }

        if (isSuspended)
        {
            await _refreshTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
        }

        return suspensionResult;
    }

    /// <inheritdoc />
    public async Task<ErrorOr<UserSuspensionResult>> SetClientSuspensionAsync(
        Guid clientUserId,
        bool isSuspended,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(clientUserId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.ClientUserNotFound",
                description: "Client user was not found.");
        }

        IList<string> roles = await _userManager.GetRolesAsync(user);
        if (!AuthPortalClassifier.IsClientPortalUser(roles))
        {
            return Error.Forbidden(
                code: "Auth.NotClientPortalUser",
                description: "The target account is not a client-portal user.");
        }

        ErrorOr<UserSuspensionResult> suspensionResult = await ApplySuspensionAsync(user, isSuspended, cancellationToken);
        if (suspensionResult.IsError)
        {
            return suspensionResult.Errors;
        }

        if (isSuspended)
        {
            await _refreshTokenRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
        }

        return suspensionResult;
    }

    private async Task<ErrorOr<UserSuspensionResult>> ApplySuspensionAsync(
        ApplicationUser user,
        bool isSuspended,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // UserManager.UpdateAsync leaves a tracked ApplicationUser with a stale ConcurrencyStamp
        // original value; SaveChanges at transaction commit then fails with DbUpdateConcurrencyException.
        DateTimeOffset? lockoutEnd = isSuspended ? DateTimeOffset.MaxValue : null;

        int updatedCount = await _dbContext.Users
            .Where(candidate => candidate.Id == user.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(candidate => candidate.LockoutEnabled, true)
                    .SetProperty(candidate => candidate.LockoutEnd, lockoutEnd)
                    .SetProperty(
                        candidate => candidate.AccessFailedCount,
                        candidate => isSuspended ? candidate.AccessFailedCount : 0)
                    .SetProperty(candidate => candidate.ConcurrencyStamp, Guid.NewGuid().ToString()),
                cancellationToken);

        DetachTrackedApplicationUser(user.Id);

        if (updatedCount == 0)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User was not found.");
        }

        string status = MapAccountStatus(isSuspended);
        return new UserSuspensionResult(user.Id, status);
    }

    private void DetachTrackedApplicationUser(Guid userId)
    {
        EntityEntry<ApplicationUser>? entry = _dbContext.ChangeTracker
            .Entries<ApplicationUser>()
            .FirstOrDefault(candidate => candidate.Entity.Id == userId);

        if (entry is not null)
        {
            entry.State = EntityState.Detached;
        }
    }

    private async Task<ErrorOr<AdminUserMutationResult>> BuildAdminMutationResultAsync(
        ApplicationUser user,
        string adminRole,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? refreshedUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (refreshedUser is null)
        {
            return AdminUserNotFound();
        }

        bool isSuspended = await _userManager.IsLockedOutAsync(refreshedUser);

        return new AdminUserMutationResult(
            Id: refreshedUser.Id,
            FullName: refreshedUser.FullName ?? string.Empty,
            Email: refreshedUser.Email!,
            AdminRole: adminRole,
            Status: MapAccountStatus(isSuspended),
            CreatedAtUtc: refreshedUser.CreatedAtUtc);
    }

    private async Task<bool> IsAdminPortalUserAsync(ApplicationUser user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);
        return AuthPortalClassifier.IsAdminPortalUser(roles);
    }

    private async Task<string> GetPrimaryAdminRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await (
            from userRole in _dbContext.UserRoles.AsNoTracking()
            join role in _dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where userRole.UserId == userId && AuthRoles.AdminRoleNames.Contains(role.Name!)
            orderby role.Name
            select role.Name!).FirstAsync(cancellationToken);
    }

    private static string MapAccountStatus(bool isSuspended) =>
        isSuspended ? "Suspended" : "Active";

    private static Error AdminUserNotFound() =>
        Error.NotFound(
            code: "Auth.AdminUserNotFound",
            description: "Admin user was not found.");

    private string GenerateSecurePassword()
    {
        return _userManager.PasswordValidators.Count > 0
            ? $"Aa1{Guid.NewGuid():N}"
            : Guid.NewGuid().ToString("N");
    }

    private async Task EnsureNewIdentityUserPersistedAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        bool exists = await _dbContext.Users.AnyAsync(
            identityUser => identityUser.Id == user.Id,
            cancellationToken);
        if (exists)
        {
            return;
        }

        EntityEntry<ApplicationUser> entry = _dbContext.Entry(user);
        if (entry.State is EntityState.Modified or EntityState.Unchanged)
        {
            entry.State = EntityState.Added;
        }
        else if (entry.State == EntityState.Detached)
        {
            _dbContext.Users.Add(user);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ErrorOr<Success>> AddUserToRoleDirectAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        ApplicationRole? role = await _roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            return Error.Failure(
                code: "Auth.RoleNotFound",
                description: $"Role '{roleName}' was not found.");
        }

        bool alreadyInRole = await _dbContext.UserRoles.AnyAsync(
            userRole => userRole.UserId == userId && userRole.RoleId == role.Id,
            cancellationToken);
        if (alreadyInRole)
        {
            return Result.Success;
        }

        _dbContext.UserRoles.Add(new IdentityUserRole<Guid>
        {
            UserId = userId,
            RoleId = role.Id
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    private static Error MapIdentityFailure(IdentityResult identityResult)
    {
        IdentityError firstError = identityResult.Errors.First();

        return Error.Validation(
            code: $"Auth.{firstError.Code}",
            description: firstError.Description);
    }

    private static Error MapPasswordFailure(IdentityResult identityResult)
    {
        IdentityError firstError = identityResult.Errors.First();

        return Error.Validation(
            code: $"Auth.{firstError.Code}",
            description: firstError.Description,
            metadata: new Dictionary<string, object> { ["field"] = "password" });
    }
}
