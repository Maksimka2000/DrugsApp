namespace DrugsApp.Domain.PatientSupervisionFeature;

using DrugsApp.Domain.PatientSupervisionFeature.Enums;

/// <summary>
/// Canonical supervision access level codes exposed to clients.
/// </summary>
public static class SupervisionAccessLevelCatalog
{
    private static readonly string[] Codes =
    [
        "view_and_modify",
        "view_only",
        "restricted_view",
        "block_access"
    ];

    /// <summary>
    /// Lists all API-facing supervision access level codes.
    /// </summary>
    /// <returns>Collection of canonical codes.</returns>
    public static IReadOnlyCollection<string> GetAllCodes() => Codes;

    /// <summary>
    /// Checks whether the provided code is a valid supervision access level.
    /// </summary>
    /// <param name="code">Code to validate.</param>
    /// <returns>True when the code is recognized.</returns>
    public static bool IsValid(string code) =>
        Codes.Contains(code, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Attempts to parse a supervision access level code.
    /// </summary>
    /// <param name="code">Code to parse.</param>
    /// <param name="accessLevel">Parsed access level when successful.</param>
    /// <returns>True when the code resolves to a known access level.</returns>
    public static bool TryParse(string? code, out SupervisionAccessLevel accessLevel)
    {
        accessLevel = default;
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        if (Enum.TryParse(code, ignoreCase: true, out accessLevel))
        {
            return true;
        }

        accessLevel = code.Trim().ToLowerInvariant() switch
        {
            "view_and_modify" => SupervisionAccessLevel.ViewAndModify,
            "view_only" => SupervisionAccessLevel.ViewOnly,
            "restricted_view" => SupervisionAccessLevel.RestrictedView,
            "block_access" => SupervisionAccessLevel.BlockAccess,
            _ => default
        };

        return IsValid(code);
    }

    /// <summary>
    /// Converts an access level enum value to the API code.
    /// </summary>
    /// <param name="accessLevel">Access level to convert.</param>
    /// <returns>API code string.</returns>
    public static string ToApiCode(SupervisionAccessLevel accessLevel) =>
        accessLevel switch
        {
            SupervisionAccessLevel.ViewAndModify => "view_and_modify",
            SupervisionAccessLevel.ViewOnly => "view_only",
            SupervisionAccessLevel.RestrictedView => "restricted_view",
            SupervisionAccessLevel.BlockAccess => "block_access",
            _ => accessLevel.ToString()
        };
}
