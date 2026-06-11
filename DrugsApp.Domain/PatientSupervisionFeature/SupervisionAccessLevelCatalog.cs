namespace DrugsApp.Domain.PatientSupervisionFeature;

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
}
