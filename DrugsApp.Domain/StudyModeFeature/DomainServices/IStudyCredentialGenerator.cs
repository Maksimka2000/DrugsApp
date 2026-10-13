using DrugsApp.Domain.StudyModeFeature.Enums;

namespace DrugsApp.Domain.StudyModeFeature.DomainServices;

/// <summary>
/// Stateless generation of login emails and passwords for provisioned study accounts.
/// </summary>
public interface IStudyCredentialGenerator
{
    /// <summary>
    /// Generates a unique-looking login email such as <c>doc-anna-nowak-k3x9qa@study.linkmed.local</c>.
    /// </summary>
    /// <param name="memberKind">
    /// Member kind selecting the <c>doc</c> or <c>pat</c> prefix.
    /// </param>
    /// <param name="firstName">
    /// Profile first name used for the readable slug.
    /// </param>
    /// <param name="lastName">
    /// Profile last name used for the readable slug.
    /// </param>
    /// <returns>
    /// Lowercase login email on the study domain.
    /// </returns>
    string GenerateLoginEmail(StudyMemberKind memberKind, string firstName, string lastName);

    /// <summary>
    /// Generates a 14-character password with uppercase, lowercase, digit and symbol characters drawn from an
    /// unambiguous alphabet.
    /// </summary>
    /// <returns>
    /// Plain generated password satisfying the Identity password policy.
    /// </returns>
    string GeneratePassword();
}
