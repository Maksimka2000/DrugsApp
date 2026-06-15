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

    public static IReadOnlyCollection<string> GetRelationshipStatusTokens() =>
        PatientHealthcareProfessionalSearchCatalog.GetRelationshipStatusTokens();

    public static IReadOnlyCollection<string> GetSortFieldTokens() => SortFieldTokens;

    public static bool TryParseRelationshipStatus(string? token, out PatientHealthcareProfessionalRelationshipStatus status) =>
        PatientHealthcareProfessionalSearchCatalog.TryParseRelationshipStatus(token, out status);

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

    public static PatientHealthcareProfessionalRelationshipStatus MapRelationshipState(string relationshipState) =>
        PatientHealthcareProfessionalSearchCatalog.MapRelationshipState(relationshipState);
}
