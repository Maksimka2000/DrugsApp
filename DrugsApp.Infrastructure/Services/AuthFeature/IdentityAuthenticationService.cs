using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.Common;
using DrugsApp.Domain.AccountFeature.Interfaces;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Enums;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Infrastructure.Persistence;
using DrugsApp.Infrastructure.Persistence.Identity;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

public sealed class IdentityAuthenticationService(
    UserManager<ApplicationUser> _userManager,
    RoleManager<ApplicationRole> _roleManager,
    DrugsAppDbContext _dbContext,
    IRefreshTokenRepository _refreshTokenRepository,
    IUserPermissionService _userPermissionService,
    IJwtTokenProvider _jwtTokenProvider,
    IEmailVerificationSender _emailVerificationSender,
    IUserProfileRepository _userProfileRepository,
    IClientReproductiveHealthProfileRepository _clientReproductiveHealthProfileRepository,
    IHealthcareProviderProfileRepository _healthcareProviderProfileRepository,
    IPendingEmailChangeRepository _pendingEmailChangeRepository,
    IClientAccountTeardownRepository _clientAccountTeardownRepository,
    IBlobService _blobService,
    IUnitOfWork _unitOfWork) : IIdentityAuthenticationService
{
    public async Task<ErrorOr<AuthRegistrationResult>> RegisterAsync(
        string email,
        string password,
        UserProfilePersonalData personalData,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim();
        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return Error.Conflict(
                code: "Auth.EmailAlreadyExists",
                description: "An account with this email already exists.",
                metadata: new Dictionary<string, object>
                {
                    ["field"] = "email"
                });
        }

        var user = new ApplicationUser
        {
            Email = normalizedEmail,
            UserName = normalizedEmail,
            CreatedAtUtc = DateTime.UtcNow
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return MapIdentityFailure(createResult);
        }

        // Identity stores use AutoSaveChanges = false. CreateAsync can leave the user in Modified
        // state (password hash update), so EF would emit UPDATE instead of INSERT at commit.
        await EnsureNewIdentityUserPersistedAsync(user, cancellationToken);
        user = (await _userManager.FindByIdAsync(user.Id.ToString()))!;

        ErrorOr<Success> assignClientRoleResult = await AddUserToRoleDirectAsync(
            user.Id,
            AuthRoles.Client,
            cancellationToken);
        if (assignClientRoleResult.IsError)
        {
            return assignClientRoleResult.Errors;
        }

        ErrorOr<UserProfile> profileResult = UserProfile.Create(
            id: user.Id,
            email: normalizedEmail,
            firstName: personalData.FirstName,
            lastName: personalData.LastName,
            dateOfBirth: personalData.DateOfBirth,
            gender: personalData.Gender);

        if (profileResult.IsError)
        {
            return profileResult.Errors;
        }

        await _userProfileRepository.AddAsync(profileResult.Value, cancellationToken);

        string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailVerificationSender.SendConfirmationAsync(
            user.Id,
            user.Email!,
            confirmationToken,
            cancellationToken);

        return new AuthRegistrationResult(
            UserId: user.Id,
            Email: user.Email!);
    }

    public async Task<ErrorOr<Success>> AssignHealthcareProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        ErrorOr<Success> assignRoleResult = await AddUserToRoleDirectAsync(
            userId,
            AuthRoles.HealthcareProvider,
            cancellationToken);
        if (assignRoleResult.IsError)
        {
            return assignRoleResult.Errors;
        }

        await _userPermissionService.InvalidateAsync(userId, cancellationToken);

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RemoveHealthcareProviderRoleAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        if (!await _userManager.IsInRoleAsync(user, AuthRoles.HealthcareProvider))
        {
            return Result.Success;
        }

        IdentityResult roleResult = await _userManager.RemoveFromRoleAsync(user, AuthRoles.HealthcareProvider);
        if (!roleResult.Succeeded)
        {
            return MapIdentityFailure(roleResult);
        }

        await _userPermissionService.InvalidateAsync(userId, cancellationToken);

        return Result.Success;
    }

    public async Task<ErrorOr<AuthTokensResult>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim();

        ApplicationUser? user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return Error.Unauthorized(
                code: "Auth.InvalidCredentials",
                description: "Invalid email or password.");
        }

        if (!user.EmailConfirmed)
        {
            return Error.Forbidden(
                code: "Auth.EmailNotConfirmed",
                description: "Email address is not confirmed.");
        }

        IList<string> roles = await _userManager.GetRolesAsync(user);
        if (AuthPortalClassifier.IsAdminPortalUser(roles))
        {
            return Error.Forbidden(
                code: "Auth.WrongPortal",
                description: "This account must sign in through the admin portal.",
                metadata: new Dictionary<string, object> { ["expectedPortal"] = "admin" });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Error.Forbidden(
                code: "Auth.AccountSuspended",
                description: "This account is suspended.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<ErrorOr<AuthTokensResult>> LoginAdminAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim();

        ApplicationUser? user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return Error.Unauthorized(
                code: "Auth.InvalidCredentials",
                description: "Invalid email or password.");
        }

        IList<string> roles = await _userManager.GetRolesAsync(user);
        if (!AuthPortalClassifier.IsAdminPortalUser(roles))
        {
            return Error.Forbidden(
                code: "Auth.WrongPortal",
                description: "This account must sign in through the client portal.",
                metadata: new Dictionary<string, object> { ["expectedPortal"] = "client" });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Error.Forbidden(
                code: "Auth.AccountSuspended",
                description: "This account is suspended.");
        }

        if (!user.EmailConfirmed)
        {
            return Error.Forbidden(
                code: "Auth.EmailNotConfirmed",
                description: "Email address is not confirmed.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<ErrorOr<AuthTokensResult>> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        RefreshToken? existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(
            refreshToken,
            cancellationToken);

        if (existingRefreshToken is null || !existingRefreshToken.IsActive(DateTime.UtcNow))
        {
            return Error.Unauthorized(
                code: "Auth.InvalidRefreshToken",
                description: "Refresh token is invalid, expired, or revoked.");
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(existingRefreshToken.UserId.ToString());
        if (user is null)
        {
            return Error.Unauthorized(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        ErrorOr<RefreshToken> rotatedRefreshTokenResult = RefreshToken.Create(
            userId: user.Id,
            expiresAtUtc: DateTime.UtcNow.AddDays(_jwtTokenProvider.GetRefreshTokenLifetimeDays()));
        if (rotatedRefreshTokenResult.IsError)
        {
            return rotatedRefreshTokenResult.Errors;
        }

        RefreshToken rotatedRefreshToken = rotatedRefreshTokenResult.Value;

        ErrorOr<Success> revokeResult = existingRefreshToken.Revoke(
            revokedAtUtc: DateTime.UtcNow,
            replacedByRefreshTokenId: rotatedRefreshToken.Id);
        if (revokeResult.IsError)
        {
            return revokeResult.Errors;
        }

        await _refreshTokenRepository.AddAsync(rotatedRefreshToken, cancellationToken);

        IList<string> roles = await _userManager.GetRolesAsync(user);
        await _userPermissionService.WarmAsync(user.Id, cancellationToken);

        string accessToken = _jwtTokenProvider.GenerateAccessToken(
            userId: user.Id,
            email: user.Email!,
            roles: roles.ToArray());

        return new AuthTokensResult(
            AccessToken: accessToken,
            RefreshToken: rotatedRefreshToken.Token,
            ExpiresInSeconds: _jwtTokenProvider.GetAccessTokenLifetimeSeconds());
    }

    public async Task<ErrorOr<Success>> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        if (user.EmailConfirmed)
        {
            return Result.Success;
        }

        IdentityResult confirmationResult = await _userManager.ConfirmEmailAsync(user, token);
        if (!confirmationResult.Succeeded)
        {
            return MapIdentityFailure(confirmationResult);
        }

        return Result.Success;
    }

    public async Task<ErrorOr<AuthRegistrationResult?>> ResendConfirmationEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim();
        ApplicationUser? user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null || user.EmailConfirmed)
        {
            return (AuthRegistrationResult?)null;
        }

        string confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailVerificationSender.SendConfirmationAsync(
            user.Id,
            user.Email!,
            confirmationToken,
            cancellationToken);

        return new AuthRegistrationResult(
            UserId: user.Id,
            Email: user.Email!);
    }

    public async Task<ErrorOr<Success>> RequestPasswordResetAsync(
        string email,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim();
        ApplicationUser? user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return Result.Success;
        }

        string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailVerificationSender.SendPasswordResetAsync(
            user.Id,
            user.Email!,
            resetToken,
            cancellationToken);

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        IdentityResult resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!resetResult.Succeeded)
        {
            return MapIdentityFailure(resetResult);
        }

        await _refreshTokenRepository.DeleteByUserIdAsync(userId, cancellationToken);

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        RefreshToken? existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(
            refreshToken,
            cancellationToken);
        if (existingRefreshToken is null)
        {
            return Result.Success;
        }

        if (existingRefreshToken.RevokedAtUtc is not null)
        {
            return Result.Success;
        }

        ErrorOr<Success> revokeResult = existingRefreshToken.Revoke(DateTime.UtcNow);
        if (revokeResult.IsError)
        {
            return revokeResult.Errors;
        }

        return Result.Success;
    }

    public async Task<ErrorOr<RequestLoginEmailChangeResult>> RequestEmailChangeAsync(
        Guid userId,
        string newEmail,
        string currentPassword,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        Error? portalError = await GetClientPortalViolationAsync(user);
        if (portalError is not null)
        {
            return portalError.Value;
        }

        if (!await _userManager.CheckPasswordAsync(user, currentPassword))
        {
            return Error.Unauthorized(
                code: "Auth.InvalidCredentials",
                description: "Current password is invalid.");
        }

        string normalizedNewEmail = newEmail.Trim().ToLowerInvariant();
        if (string.Equals(user.Email, normalizedNewEmail, StringComparison.OrdinalIgnoreCase))
        {
            return Error.Conflict(
                code: "Auth.EmailUnchanged",
                description: "New email matches the current email.");
        }

        ApplicationUser? conflictUser = await _userManager.FindByEmailAsync(normalizedNewEmail);
        if (conflictUser is not null && conflictUser.Id != user.Id)
        {
            return Error.Conflict(
                code: "Auth.EmailAlreadyExists",
                description: "An account with this email already exists.",
                metadata: new()
                {
                    ["field"] = "email"
                });
        }

        string confirmationToken = await _userManager.GenerateChangeEmailTokenAsync(user, normalizedNewEmail);
        string tokenHash = confirmationToken;

        PendingEmailChange? existingPending = await _pendingEmailChangeRepository.GetByUserIdAsync(
            user.Id,
            cancellationToken);

        DateTime expiresAtUtc = DateTime.UtcNow.AddHours(24);

        if (existingPending is null)
        {
            ErrorOr<PendingEmailChange> pendingResult = PendingEmailChange.Create(
                user.Id,
                normalizedNewEmail,
                tokenHash,
                expiresAtUtc);

            if (pendingResult.IsError)
            {
                return pendingResult.Errors;
            }

            await _pendingEmailChangeRepository.AddAsync(pendingResult.Value, cancellationToken);
        }
        else if (existingPending.IsExpired())
        {
            _pendingEmailChangeRepository.Remove(existingPending);

            ErrorOr<PendingEmailChange> pendingResult = PendingEmailChange.Create(
                user.Id,
                normalizedNewEmail,
                tokenHash,
                expiresAtUtc);

            if (pendingResult.IsError)
            {
                return pendingResult.Errors;
            }

            await _pendingEmailChangeRepository.AddAsync(pendingResult.Value, cancellationToken);
        }
        else
        {
            ErrorOr<Success> replaceResult = existingPending.Replace(
                normalizedNewEmail,
                tokenHash,
                expiresAtUtc);
            if (replaceResult.IsError)
            {
                return replaceResult.Errors;
            }
        }

        await _emailVerificationSender.SendEmailChangeConfirmationAsync(
            user.Id,
            normalizedNewEmail,
            confirmationToken,
            cancellationToken);

        return new RequestLoginEmailChangeResult(
            Email: user.Email!,
            PendingEmail: normalizedNewEmail,
            VerificationRequired: true);
    }

    public async Task<ErrorOr<ConfirmLoginEmailChangeResult>> ConfirmEmailChangeAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        Error? portalError = await GetClientPortalViolationAsync(user);
        if (portalError is not null)
        {
            return portalError.Value;
        }

        PendingEmailChange? pending = await _pendingEmailChangeRepository.GetActiveByUserIdAsync(
            userId,
            cancellationToken);

        if (pending is null)
        {
            return Error.NotFound(
                code: "Auth.PendingEmailChangeNotFound",
                description: "No pending email change found.");
        }

        if (pending.IsExpired())
        {
            _pendingEmailChangeRepository.Remove(pending);
            return Error.Conflict(
                code: "Auth.PendingEmailChangeExpired",
                description: "Email change request expired.");
        }

        IdentityResult changeResult = await _userManager.ChangeEmailAsync(
            user,
            pending.NewEmail,
            token);
        if (!changeResult.Succeeded)
        {
            return MapIdentityFailure(changeResult);
        }

        IdentityResult usernameResult = await _userManager.SetUserNameAsync(user, pending.NewEmail);
        if (!usernameResult.Succeeded)
        {
            return MapIdentityFailure(usernameResult);
        }

        _pendingEmailChangeRepository.Remove(pending);

        return new ConfirmLoginEmailChangeResult(
            Email: pending.NewEmail,
            EmailChangeConfirmed: true);
    }

    public async Task<ErrorOr<Success>> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        Error? portalError = await GetClientPortalViolationAsync(user);
        if (portalError is not null)
        {
            return portalError.Value;
        }

        IdentityResult result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return MapIdentityFailure(result);
        }

        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeleteAccountAsync(
        Guid userId,
        string currentPassword,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        Error? portalError = await GetClientPortalViolationAsync(user);
        if (portalError is not null)
        {
            return portalError.Value;
        }

        if (!await _userManager.CheckPasswordAsync(user, currentPassword))
        {
            return Error.Unauthorized(
                code: "Auth.InvalidCredentials",
                description: "Current password is invalid.");
        }

        return await TeardownClientAccountAsync(userId, cancellationToken);
    }

    public async Task<ErrorOr<Success>> TeardownClientAccountAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error.NotFound(
                code: "Auth.UserNotFound",
                description: "User account was not found.");
        }

        IList<string> roles = await _userManager.GetRolesAsync(user);

        UserProfile? profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        string? photoKey = profile?.ProfilePhotoStorageKey;

        await _clientAccountTeardownRepository.DeleteAllOwnedClientDataAsync(userId, cancellationToken);
        await _userProfileRepository.DeleteByUserIdAsync(userId, cancellationToken);
        await _clientReproductiveHealthProfileRepository.RemoveByUserIdAsync(userId, cancellationToken);
        await _healthcareProviderProfileRepository.RemoveByUserIdAsync(userId, cancellationToken);
        await _pendingEmailChangeRepository.RemoveByUserIdAsync(userId, cancellationToken);
        await _refreshTokenRepository.DeleteByUserIdAsync(userId, cancellationToken);

        if (photoKey is not null && Guid.TryParse(photoKey, out Guid storageKey))
        {
            await _blobService.DeleteAsync(storageKey, cancellationToken);
        }

        IdentityResult deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            return MapIdentityFailure(deleteResult);
        }

        if (roles.Contains(AuthRoles.HealthcareProvider))
        {
            await _userPermissionService.InvalidateAsync(userId, cancellationToken);
        }

        return Result.Success;
    }

    private async Task<ErrorOr<AuthTokensResult>> IssueTokensAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);
        await _userPermissionService.WarmAsync(user.Id, cancellationToken);

        ErrorOr<RefreshToken> refreshTokenResult = RefreshToken.Create(
            userId: user.Id,
            expiresAtUtc: DateTime.UtcNow.AddDays(_jwtTokenProvider.GetRefreshTokenLifetimeDays()));
        if (refreshTokenResult.IsError)
        {
            return refreshTokenResult.Errors;
        }

        RefreshToken refreshToken = refreshTokenResult.Value;
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        string accessToken = _jwtTokenProvider.GenerateAccessToken(
            userId: user.Id,
            email: user.Email!,
            roles: roles.ToArray());

        return new AuthTokensResult(
            AccessToken: accessToken,
            RefreshToken: refreshToken.Token,
            ExpiresInSeconds: _jwtTokenProvider.GetAccessTokenLifetimeSeconds());
    }

    private async Task<Error?> GetClientPortalViolationAsync(ApplicationUser user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);
        if (AuthPortalClassifier.IsClientPortalUser(roles))
        {
            return null;
        }

        return Error.Forbidden(
            code: "Auth.WrongPortal",
            description: "This action is only available for client portal accounts.",
            metadata: new Dictionary<string, object> { ["expectedPortal"] = "client" });
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
}
