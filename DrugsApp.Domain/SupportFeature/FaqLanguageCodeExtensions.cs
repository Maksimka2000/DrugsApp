using System;
using DrugsApp.Domain.SupportFeature.Enums;

namespace DrugsApp.Domain.SupportFeature;

/// <summary>
/// Parsing and formatting helpers for <see cref="FaqLanguageCode"/>.
/// </summary>
public static class FaqLanguageCodeExtensions
{
    /// <summary>
    /// Converts a language code enum value to its route and persistence code.
    /// </summary>
    public static string ToCode(this FaqLanguageCode languageCode) => languageCode switch
    {
        FaqLanguageCode.En => "en",
        FaqLanguageCode.Pl => "pl",
        _ => throw new ArgumentOutOfRangeException(nameof(languageCode), languageCode, "Unsupported FAQ language code.")
    };

    /// <summary>
    /// Parses a route or persistence language code into the enum value.
    /// </summary>
    public static bool TryParseCode(string? code, out FaqLanguageCode languageCode)
    {
        if (string.Equals(code, "en", StringComparison.OrdinalIgnoreCase))
        {
            languageCode = FaqLanguageCode.En;
            return true;
        }

        if (string.Equals(code, "pl", StringComparison.OrdinalIgnoreCase))
        {
            languageCode = FaqLanguageCode.Pl;
            return true;
        }

        languageCode = default;
        return false;
    }

    /// <summary>
    /// Parses a route or persistence language code into the enum value.
    /// </summary>
    public static FaqLanguageCode ParseCode(string code)
    {
        if (TryParseCode(code, out FaqLanguageCode languageCode))
        {
            return languageCode;
        }

        throw new ArgumentException($"Unsupported FAQ language code '{code}'.", nameof(code));
    }
}
