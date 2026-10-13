using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DrugsApp.Domain.StudyModeFeature.Enums;

namespace DrugsApp.Domain.StudyModeFeature.DomainServices;

/// <summary>
/// Stateless credential generator for study accounts backed by <see cref="RandomNumberGenerator"/>; produces
/// readable login emails on the study domain and strong passwords from an unambiguous alphabet.
/// </summary>
public sealed class StudyCredentialGenerator : IStudyCredentialGenerator
{
    private const string LOGIN_DOMAIN = "study.linkmed.local";
    private const int SLUG_MAX_LENGTH = 40;
    private const int SUFFIX_LENGTH = 6;
    private const int PASSWORD_LENGTH = 14;
    private const string SUFFIX_ALPHABET = "abcdefghijkmnpqrstuvwxyz23456789";
    private const string UPPERCASE_ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LOWERCASE_ALPHABET = "abcdefghijkmnpqrstuvwxyz";
    private const string DIGIT_ALPHABET = "23456789";
    private const string SYMBOL_ALPHABET = "!@#$%^&*-_=+?";

    /// <summary>
    /// Generates a login email composed of the kind prefix, a name slug and a random suffix.
    /// </summary>
    /// <param name="memberKind">
    /// Member kind selecting the <c>doc</c> or <c>pat</c> prefix.
    /// </param>
    /// <param name="firstName">
    /// Profile first name.
    /// </param>
    /// <param name="lastName">
    /// Profile last name.
    /// </param>
    /// <returns>
    /// Lowercase login email on the study domain.
    /// </returns>
    public string GenerateLoginEmail(StudyMemberKind memberKind, string firstName, string lastName)
    {
        string prefix = memberKind == StudyMemberKind.Doctor ? "doc" : "pat";
        string slug = BuildSlug($"{firstName} {lastName}");
        string suffix = PickRandomCharacters(SUFFIX_ALPHABET, SUFFIX_LENGTH);

        return $"{prefix}-{slug}-{suffix}@{LOGIN_DOMAIN}";
    }

    /// <summary>
    /// Generates a shuffled password guaranteeing at least one uppercase, lowercase, digit and symbol character.
    /// </summary>
    /// <returns>
    /// Plain generated password.
    /// </returns>
    public string GeneratePassword()
    {
        string combinedAlphabet = UPPERCASE_ALPHABET + LOWERCASE_ALPHABET + DIGIT_ALPHABET + SYMBOL_ALPHABET;

        List<char> passwordCharacters =
        [
            PickRandomCharacter(UPPERCASE_ALPHABET),
            PickRandomCharacter(LOWERCASE_ALPHABET),
            PickRandomCharacter(DIGIT_ALPHABET),
            PickRandomCharacter(SYMBOL_ALPHABET)
        ];

        while (passwordCharacters.Count < PASSWORD_LENGTH)
        {
            passwordCharacters.Add(PickRandomCharacter(combinedAlphabet));
        }

        for (int index = passwordCharacters.Count - 1; index > 0; index--)
        {
            int swapIndex = RandomNumberGenerator.GetInt32(index + 1);
            (passwordCharacters[index], passwordCharacters[swapIndex]) = (passwordCharacters[swapIndex], passwordCharacters[index]);
        }

        return new string(passwordCharacters.ToArray());
    }

    private static string BuildSlug(string value)
    {
        string decomposed = value
            .Replace('ł', 'l')
            .Replace('Ł', 'L')
            .Normalize(NormalizationForm.FormD);

        var slugBuilder = new StringBuilder();
        bool previousWasSeparator = true;
        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            char lowered = char.ToLowerInvariant(character);
            bool isAsciiAlphanumeric = lowered is >= 'a' and <= 'z' or >= '0' and <= '9';
            if (isAsciiAlphanumeric)
            {
                slugBuilder.Append(lowered);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator)
            {
                slugBuilder.Append('-');
                previousWasSeparator = true;
            }
        }

        string slug = slugBuilder.ToString().Trim('-');
        if (slug.Length > SLUG_MAX_LENGTH)
        {
            slug = slug[..SLUG_MAX_LENGTH].TrimEnd('-');
        }

        return slug.Length == 0 ? "user" : slug;
    }

    private static string PickRandomCharacters(string alphabet, int length)
    {
        var characters = new char[length];
        for (int index = 0; index < length; index++)
        {
            characters[index] = PickRandomCharacter(alphabet);
        }

        return new string(characters);
    }

    private static char PickRandomCharacter(string alphabet) =>
        alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
}
