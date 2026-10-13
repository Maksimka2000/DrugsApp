using ErrorOr;

namespace DrugsApp.Domain.StudyModeFeature.Interfaces;

/// <summary>
/// Reversible protection of generated study account passwords so the teacher can re-read them later.
/// </summary>
public interface IStudyCredentialProtector
{
    /// <summary>
    /// Encrypts a plain generated password.
    /// </summary>
    /// <param name="password">
    /// Plain generated password.
    /// </param>
    /// <returns>
    /// Protected payload safe to persist.
    /// </returns>
    string Protect(string password);

    /// <summary>
    /// Decrypts a previously protected password.
    /// </summary>
    /// <param name="protectedPassword">
    /// Persisted protected payload.
    /// </param>
    /// <returns>
    /// The plain password, or an unexpected error with code <c>StudyMode.CredentialsUnavailable</c> when the key ring
    /// can no longer decrypt the payload.
    /// </returns>
    ErrorOr<string> Unprotect(string protectedPassword);
}
