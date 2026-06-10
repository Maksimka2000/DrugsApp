using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Domain.AuthFeature.Entities;
using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.Common;
using DrugsApp.Infrastructure.Persistence.Identity;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace DrugsApp.Infrastructure.Services.AuthFeature;

public sealed class IdentityAuthenticationService(
    UserManager<ApplicationUser> _userManager,
    IRefreshTokenRepository _refreshTokenRepository,
    IUserPermissionService _userPermissionService,
    IJwtTokenProvider _jwtTokenProvider,
    IEmailVerificationSender _emailVerificationSender,
    IUnitOfWork _unitOfWork) : IIdentityAuthenticationService
{
    public async Task<ErrorOr<AuthRegistrationResult>> RegisterAsync(
        string email,
        string password,
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
            UserName = normalizedEmail
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return MapIdentityFailure(createResult);
        }

        // Identity stores use AutoSaveChanges = false. Persist the user before adding
        // the role link so SaveChanges does not insert AspNetUserRoles before AspNetUsers.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, AuthRoles.Client);
        if (!roleResult.Succeeded)
        {
            return MapIdentityFailure(roleResult);
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

    private static Error MapIdentityFailure(IdentityResult identityResult)
    {
        IdentityError firstError = identityResult.Errors.First();

        return Error.Validation(
            code: $"Auth.{firstError.Code}",
            description: firstError.Description);
    }
}
