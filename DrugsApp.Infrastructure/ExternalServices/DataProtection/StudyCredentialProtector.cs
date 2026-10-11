using System.Security.Cryptography;
using DrugsApp.Domain.StudyModeFeature.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.DataProtection;

namespace DrugsApp.Infrastructure.ExternalServices.DataProtection;

/// <summary>
/// ASP.NET Core Data Protection adapter that encrypts generated study account passwords with a purpose-bound
/// protector whose key ring is persisted in PostgreSQL.
/// </summary>
internal sealed class StudyCredentialProtector(IDataProtectionProvider dataProtectionProvider) : IStudyCredentialProtector
{
    private const string PROTECTOR_PURPOSE = "StudyMode.Credentials.v1";

    private readonly IDataProtector _dataProtector = dataProtectionProvider.CreateProtector(PROTECTOR_PURPOSE);

    /// <summary>
    /// Encrypts a plain generated password.
    /// </summary>
    /// <param name="password">
    /// Plain generated password.
    /// </param>
    /// <returns>
    /// Protected payload safe to persist.
    /// </returns>
    public string Protect(string password) => _dataProtector.Protect(password);

    /// <summary>
    /// Decrypts a previously protected password, translating a lost or rotated-away key ring into an expected error.
    /// </summary>
    /// <param name="protectedPassword">
    /// Persisted protected payload.
    /// </param>
    /// <returns>
    /// The plain password or <c>StudyMode.CredentialsUnavailable</c>.
    /// </returns>
    public ErrorOr<string> Unprotect(string protectedPassword)
    {
        try
        {
            return _dataProtector.Unprotect(protectedPassword);
        }
        catch (CryptographicException)
        {
            return Error.Unexpected(
                code: "StudyMode.CredentialsUnavailable",
                description: "The stored study account credentials can no longer be decrypted.");
        }
    }
}
