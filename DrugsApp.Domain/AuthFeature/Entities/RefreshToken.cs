using System;
using System.Security.Cryptography;
using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.AuthFeature.Entities;

/// <summary>
/// Represents a refresh token issued for bearer-token renewal.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    // Attributes

    /// <summary>
    /// Gets the owning identity user identifier.
    /// </summary>
    /// <remarks>
    /// This value references an ASP.NET Core Identity user without introducing the persistence type into Domain.
    /// </remarks>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the opaque refresh token value sent to the client.
    /// </summary>
    /// <remarks>
    /// The value is randomly generated and treated as a bearer secret.
    /// </remarks>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the UTC expiration instant for the token.
    /// </summary>
    /// <remarks>
    /// Expired tokens must not be accepted for access-token renewal.
    /// </remarks>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC instant when the token was revoked.
    /// </summary>
    /// <remarks>
    /// A non-null value means the token must be treated as invalid.
    /// </remarks>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Gets the replacement refresh token identifier if the token was rotated.
    /// </summary>
    /// <remarks>
    /// This value links token rotation events for auditability.
    /// </remarks>
    public Guid? ReplacedByRefreshTokenId { get; private set; }

    // Relationships

    private RefreshToken() : base()
    {
    }

    private RefreshToken(
        Guid userId,
        string token,
        DateTime expiresAtUtc) : this()
    {
        UserId = userId;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    // Methods

    /// <summary>
    /// Creates a new refresh token aggregate.
    /// </summary>
    /// <param name="userId">Owning identity user identifier.</param>
    /// <param name="expiresAtUtc">UTC expiration instant.</param>
    /// <returns>
    /// A new <see cref="RefreshToken"/> instance or validation errors.
    /// </returns>
    public static ErrorOr<RefreshToken> Create(
        Guid userId,
        DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "RefreshToken.UserIdRequired",
                description: "Refresh token user id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        if (expiresAtUtc <= DateTime.UtcNow)
        {
            return Error.Validation(
                code: "RefreshToken.ExpiresAtUtcInvalid",
                description: "Refresh token expiration must be in the future.",
                metadata: new()
                {
                    ["field"] = "expiresAtUtc"
                });
        }

        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new RefreshToken(
            userId: userId,
            token: token,
            expiresAtUtc: expiresAtUtc);
    }

    /// <summary>
    /// Returns whether the token is expired at the given instant.
    /// </summary>
    /// <param name="utcNow">Current UTC instant.</param>
    /// <returns>
    /// <c>true</c> when the token has expired; otherwise <c>false</c>.
    /// </returns>
    public bool IsExpired(DateTime utcNow) => ExpiresAtUtc <= utcNow;

    /// <summary>
    /// Returns whether the token has already been revoked.
    /// </summary>
    /// <param name="utcNow">Current UTC instant.</param>
    /// <returns>
    /// <c>true</c> when the token is active and eligible for use; otherwise <c>false</c>.
    /// </returns>
    public bool IsActive(DateTime utcNow) => RevokedAtUtc is null && !IsExpired(utcNow);

    /// <summary>
    /// Revokes the token and optionally links it to a replacement token.
    /// </summary>
    /// <param name="revokedAtUtc">UTC revocation instant.</param>
    /// <param name="replacedByRefreshTokenId">Optional replacement token identifier.</param>
    /// <returns>
    /// Success when the token can be revoked; otherwise validation errors.
    /// </returns>
    public ErrorOr<Success> Revoke(
        DateTime revokedAtUtc,
        Guid? replacedByRefreshTokenId = null)
    {
        if (RevokedAtUtc is not null)
        {
            return Error.Conflict(
                code: "RefreshToken.AlreadyRevoked",
                description: "Refresh token has already been revoked.");
        }

        if (revokedAtUtc < CreatedAtUtc)
        {
            return Error.Validation(
                code: "RefreshToken.RevokedAtUtcInvalid",
                description: "Refresh token revocation time is invalid.");
        }

        RevokedAtUtc = revokedAtUtc;
        ReplacedByRefreshTokenId = replacedByRefreshTokenId;

        return Result.Success;
    }
}
