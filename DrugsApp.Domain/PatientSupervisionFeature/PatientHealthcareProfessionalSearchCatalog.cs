using DrugsApp.Domain.PatientSupervisionFeature.Enums;

namespace DrugsApp.Domain.PatientSupervisionFeature;

/// <summary>
/// Canonical tokens for patient healthcare professional list search and filtering.
/// </summary>
public static class PatientHealthcareProfessionalSearchCatalog
{
    private static readonly string[] RelationshipStatusTokens =
    [
        "pending",
        "active",
        "rejected",
        "cancelled",
        "expired",
        "removed"
    ];

    private static readonly string[] SortFieldTokens =
    [
        "status",
        "date_added",
        "display_name"
    ];

    /// <summary>
    /// Returns canonical relationship status tokens usable in patient list filters.
    /// </summary>
    /// <returns>Allowed status tokens in normalized form.</returns>
    public static IReadOnlyCollection<string> GetRelationshipStatusTokens() => RelationshipStatusTokens;

    /// <summary>
    /// Returns canonical sort field tokens usable in patient list ordering.
    /// </summary>
    /// <returns>Allowed sort tokens in normalized form.</returns>
    public static IReadOnlyCollection<string> GetSortFieldTokens() => SortFieldTokens;

    /// <summary>
    /// Attempts to parse a relationship status token to the corresponding enum value.
    /// </summary>
    /// <param name="token">Normalized or raw status token.</param>
    /// <param name="status">Parsed relationship status when successful.</param>
    /// <returns><c>true</c> when parsed; otherwise <c>false</c>.</returns>
    public static bool TryParseRelationshipStatus(string? token, out PatientHealthcareProfessionalRelationshipStatus status)
    {
        status = default;
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return token.Trim().ToLowerInvariant() switch
        {
            "pending" => Assign(PatientHealthcareProfessionalRelationshipStatus.Pending, out status),
            "active" => Assign(PatientHealthcareProfessionalRelationshipStatus.Active, out status),
            "rejected" => Assign(PatientHealthcareProfessionalRelationshipStatus.Rejected, out status),
            "cancelled" => Assign(PatientHealthcareProfessionalRelationshipStatus.Cancelled, out status),
            "expired" => Assign(PatientHealthcareProfessionalRelationshipStatus.Expired, out status),
            "removed" => Assign(PatientHealthcareProfessionalRelationshipStatus.Removed, out status),
            _ => false
        };
    }

    /// <summary>
    /// Attempts to parse a sort token to the corresponding sort field enum value.
    /// </summary>
    /// <param name="token">Normalized or raw sort token.</param>
    /// <param name="sortField">Parsed sort field when successful.</param>
    /// <returns><c>true</c> when parsed or when token is empty; otherwise <c>false</c>.</returns>
    public static bool TryParseSortField(string? token, out PatientHealthcareProfessionalSortField sortField)
    {
        sortField = PatientHealthcareProfessionalSortField.Status;
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
            "date_added" => PatientHealthcareProfessionalSortField.DateAdded,
            "display_name" => PatientHealthcareProfessionalSortField.DisplayName,
            _ => PatientHealthcareProfessionalSortField.Status
        };

        return true;
    }

    /// <summary>
    /// Maps a relationship state name to the normalized enum representation.
    /// </summary>
    /// <param name="relationshipState">Relationship state string to map.</param>
    /// <returns>Corresponding relationship status enum value.</returns>
    public static PatientHealthcareProfessionalRelationshipStatus MapRelationshipState(string relationshipState)
    {
        return relationshipState switch
        {
            nameof(PatientHealthcareProfessionalRelationshipStatus.Pending) =>
                PatientHealthcareProfessionalRelationshipStatus.Pending,
            nameof(PatientHealthcareProfessionalRelationshipStatus.Active) =>
                PatientHealthcareProfessionalRelationshipStatus.Active,
            nameof(PatientHealthcareProfessionalRelationshipStatus.Rejected) =>
                PatientHealthcareProfessionalRelationshipStatus.Rejected,
            nameof(PatientHealthcareProfessionalRelationshipStatus.Cancelled) =>
                PatientHealthcareProfessionalRelationshipStatus.Cancelled,
            nameof(PatientHealthcareProfessionalRelationshipStatus.Expired) =>
                PatientHealthcareProfessionalRelationshipStatus.Expired,
            nameof(PatientHealthcareProfessionalRelationshipStatus.Removed) =>
                PatientHealthcareProfessionalRelationshipStatus.Removed,
            _ => Enum.Parse<PatientHealthcareProfessionalRelationshipStatus>(relationshipState, ignoreCase: true)
        };
    }

    private static bool Assign(
        PatientHealthcareProfessionalRelationshipStatus value,
        out PatientHealthcareProfessionalRelationshipStatus status)
    {
        status = value;
        return true;
    }
}
