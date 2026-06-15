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

    public static IReadOnlyCollection<string> GetAllCodes() => Codes;

    public static bool IsValid(string code) =>
        Codes.Contains(code, StringComparer.OrdinalIgnoreCase);

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
