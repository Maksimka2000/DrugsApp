using DrugsApp.Domain.PatientSupervisionFeature.Enums;

namespace DrugsApp.Domain.PatientSupervisionFeature;

/// <summary>
/// Canonical tokens for provider patient list search and filtering.
/// </summary>
public static class ProviderPatientSearchCatalog
{
    private static readonly string[] SortFieldTokens =
    [
        "status",
        "date_added",
        "display_name"
    ];

    /// <summary>
    /// Returns canonical relationship status tokens usable in provider-facing patient filters.
    /// </summary>
    /// <returns>Allowed status tokens in normalized form.</returns>
    public static IReadOnlyCollection<string> GetRelationshipStatusTokens() =>
        PatientHealthcareProfessionalSearchCatalog.GetRelationshipStatusTokens();

    /// <summary>
    /// Returns canonical sort field tokens usable in provider-facing patient ordering.
    /// </summary>
    /// <returns>Allowed sort tokens in normalized form.</returns>
    public static IReadOnlyCollection<string> GetSortFieldTokens() => SortFieldTokens;

    /// <summary>
    /// Attempts to parse a relationship status token to the corresponding enum value.
    /// </summary>
    /// <param name="token">Normalized or raw relationship status token.</param>
    /// <param name="status">Parsed relationship status when successful.</param>
    /// <returns><c>true</c> when parsed; otherwise <c>false</c>.</returns>
    public static bool TryParseRelationshipStatus(string? token, out PatientHealthcareProfessionalRelationshipStatus status) =>
        PatientHealthcareProfessionalSearchCatalog.TryParseRelationshipStatus(token, out status);

    /// <summary>
    /// Attempts to parse a sort token to the corresponding provider sort field enum value.
    /// </summary>
    /// <param name="token">Normalized or raw sort token.</param>
    /// <param name="sortField">Parsed sort field when successful.</param>
    /// <returns><c>true</c> when parsed or when token is empty; otherwise <c>false</c>.</returns>
    public static bool TryParseSortField(string? token, out ProviderPatientSortField sortField)
    {
        sortField = ProviderPatientSortField.Status;
        if (string.IsNullOrWhiteSpace(token))
        {
            return true;
        }

        string normalized = token.Trim().ToLowerInvariant();
        if (!SortFieldTokens.Contains(normalized))
        {
            return false;
        }

        sortField = normalized switch
        {
            "date_added" => ProviderPatientSortField.DateAdded,
            "display_name" => ProviderPatientSortField.DisplayName,
            _ => ProviderPatientSortField.Status
        };

        return true;
    }

    /// <summary>
    /// Maps a relationship state string to the normalized enum representation.
    /// </summary>
    /// <param name="relationshipState">Relationship state name to map.</param>
    /// <returns>Corresponding relationship status enum value.</returns>
    public static PatientHealthcareProfessionalRelationshipStatus MapRelationshipState(string relationshipState) =>
        PatientHealthcareProfessionalSearchCatalog.MapRelationshipState(relationshipState);
}
