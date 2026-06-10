using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.AuthFeature.Entities;

/// <summary>
/// Staged login email change awaiting confirmation.
/// </summary>
public sealed class PendingEmailChange : BaseEntity
{
    // Attributes

    /// <summary>
    /// Owner user identifier.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Normalized new email awaiting verification.
    /// </summary>
    public string NewEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Hashed confirmation token associated with the pending change.
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>
    /// Expiration timestamp for the pending change.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    // Relationships

    private PendingEmailChange()
    {
    }

    private PendingEmailChange(
        Guid userId,
        string newEmail,
        string tokenHash,
        DateTime expiresAtUtc) : this()
    {
        UserId = userId;
        NewEmail = newEmail;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    // Factory and behaviors

    /// <summary>
    /// Creates a pending email change for the specified user.
    /// </summary>
    /// <param name="userId">Owner user identifier.</param>
    /// <param name="newEmail">Target login email.</param>
    /// <param name="tokenHash">Hashed confirmation token.</param>
    /// <param name="expiresAtUtc">Expiration timestamp for the pending change.</param>
    /// <returns>Validated <see cref="PendingEmailChange"/> or validation errors.</returns>
    public static ErrorOr<PendingEmailChange> Create(
        Guid userId,
        string newEmail,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        ErrorOr<ValidatedFields> validated = Validate(userId, newEmail, tokenHash, expiresAtUtc, DateTime.UtcNow);
        if (validated.IsError)
        {
            return validated.Errors;
        }

        return new PendingEmailChange(
            validated.Value.UserId,
            validated.Value.NewEmail,
            validated.Value.TokenHash,
            validated.Value.ExpiresAtUtc);
    }

    /// <summary>
    /// Replaces the pending email change payload with a new token and email.
    /// </summary>
    /// <param name="newEmail">Target login email.</param>
    /// <param name="tokenHash">New hashed token.</param>
    /// <param name="expiresAtUtc">New expiration timestamp.</param>
    /// <returns>Success or validation errors.</returns>
    public ErrorOr<Success> Replace(string newEmail, string tokenHash, DateTime expiresAtUtc)
    {
        ErrorOr<ValidatedFields> validated = Validate(UserId, newEmail, tokenHash, expiresAtUtc, DateTime.UtcNow);
        if (validated.IsError)
        {
            return validated.Errors;
        }

        NewEmail = validated.Value.NewEmail;
        TokenHash = validated.Value.TokenHash;
        ExpiresAtUtc = validated.Value.ExpiresAtUtc;

        return Result.Success;
    }

    /// <summary>
    /// Indicates whether the pending change is expired.
    /// </summary>
    /// <returns>True when expired; otherwise false.</returns>
    public bool IsExpired() => DateTime.UtcNow >= ExpiresAtUtc;

    private static ErrorOr<ValidatedFields> Validate(
        Guid userId,
        string newEmail,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime nowUtc)
    {
        if (userId == Guid.Empty)
        {
            return Error.Validation(
                code: "PendingEmailChange.UserIdEmpty",
                description: "User id must be provided.",
                metadata: new()
                {
                    ["field"] = "userId"
                });
        }

        ErrorOr<string> normalizedEmail = NormalizeEmail(newEmail);
        if (normalizedEmail.IsError)
        {
            return normalizedEmail.Errors;
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return Error.Validation(
                code: "PendingEmailChange.TokenHashEmpty",
                description: "Token hash must be provided.",
                metadata: new()
                {
                    ["field"] = "tokenHash"
                });
        }

        if (expiresAtUtc <= nowUtc)
        {
            return Error.Validation(
                code: "PendingEmailChange.ExpiresAtInPast",
                description: "Expiration time must be in the future.",
                metadata: new()
                {
                    ["field"] = "expiresAtUtc"
                });
        }

        return new ValidatedFields(
            userId,
            normalizedEmail.Value,
            tokenHash.Trim(),
            expiresAtUtc);
    }

    private static ErrorOr<string> NormalizeEmail(string value)
    {
        string trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return Error.Validation(
                code: "PendingEmailChange.EmailEmpty",
                description: "Email must be provided.",
                metadata: new()
                {
                    ["field"] = "newEmail"
                });
        }

        if (trimmed.Length > 256)
        {
            return Error.Validation(
                code: "PendingEmailChange.EmailTooLong",
                description: "Email must not exceed 256 characters.",
                metadata: new()
                {
                    ["field"] = "newEmail",
                    ["maxLength"] = 256
                });
        }

        return trimmed.ToLowerInvariant();
    }

    private sealed record ValidatedFields(
        Guid UserId,
        string NewEmail,
        string TokenHash,
        DateTime ExpiresAtUtc);
}
