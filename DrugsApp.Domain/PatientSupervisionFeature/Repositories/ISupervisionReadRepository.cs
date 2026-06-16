using DrugsApp.Domain.PatientSupervisionFeature.Entities;

using DrugsApp.Domain.PatientSupervisionFeature.Enums;

namespace DrugsApp.Domain.PatientSupervisionFeature.Repositories;

public interface ISupervisionReadRepository
{
    Task<SearchPatientHealthcareProfessionalsResult> SearchPatientHealthcareProfessionalsAsync(
        Guid patientUserId,
        string? searchQuery,
        PatientHealthcareProfessionalSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        PatientHealthcareProfessionalSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountPatientHealthcareProfessionalsAsync(
        Guid patientUserId,
        string? searchQuery,
        PatientHealthcareProfessionalSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        CancellationToken cancellationToken);

    Task<PatientHealthcareProfessionalDetail?> GetPatientHealthcareProfessionalDetailAsync(
        Guid patientUserId,
        Guid healthcareProfessionalUserId,
        CancellationToken cancellationToken);

    Task<SearchProviderPatientsResult> SearchProviderPatientsAsync(
        Guid providerUserId,
        string? searchQuery,
        ProviderPatientSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        ProviderPatientSortField sortField,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountProviderPatientsAsync(
        Guid providerUserId,
        string? searchQuery,
        ProviderPatientSearchField searchField,
        IReadOnlyCollection<PatientHealthcareProfessionalRelationshipStatus>? statuses,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PatientSupervisionRequestListRow>> ListPatientSupervisionRequestsAsync(
        Guid patientUserId,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountPatientSupervisionRequestsAsync(
        Guid patientUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProviderSupervisionRequestListRow>> ListProviderSupervisionRequestsAsync(
        Guid providerUserId,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountProviderSupervisionRequestsAsync(
        Guid providerUserId,
        CancellationToken cancellationToken);
}

public sealed record PatientHealthcareProfessionalListRow(
    string EntryType,
    Guid HealthcareProfessionalUserId,
    Guid? RequestId,
    Guid? ConnectionId,
    string RelationshipState,
    string? AccessLevelCode,
    string Email,
    string FirstName,
    string LastName,
    string? ProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    bool HasPendingProposal,
    int? PendingProposalCount,
    DateTime RequestedAtUtc,
    DateTime? ApprovedAtUtc,
    string? ProfilePhotoStorageKey);

public sealed record PatientHealthcareProfessionalDetail(
    Guid HealthcareProfessionalUserId,
    string? ProfilePhotoStorageKey,
    string FirstName,
    string LastName,
    string? ProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    string RelationshipState,
    string? AccessLevelCode,
    bool HasPendingProposal,
    int? PendingProposalCount,
    Guid? ActiveRequestId,
    Guid? ActiveConnectionId);

public sealed record SearchPatientHealthcareProfessionalsResult(
    IReadOnlyCollection<PatientHealthcareProfessionalListRow> Items,
    int TotalCount);

public sealed record ProviderPatientListRow(
    string EntryType,
    Guid PatientUserId,
    Guid? RequestId,
    Guid? ConnectionId,
    string RelationshipState,
    string? AccessLevelCode,
    string Email,
    string FirstName,
    string LastName,
    bool HasProfilePhoto,
    DateTime RequestedAtUtc,
    DateTime? ApprovedAtUtc);

public sealed record SearchProviderPatientsResult(
    IReadOnlyCollection<ProviderPatientListRow> Items,
    int TotalCount);

public sealed record PatientSupervisionRequestListRow(
    Guid RequestId,
    Guid HealthcareProfessionalUserId,
    SupervisionRequestState RequestState,
    DateTime RequestedAtUtc,
    string? ProfilePhotoStorageKey,
    string FirstName,
    string LastName,
    string? ProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription);

public sealed record ProviderSupervisionRequestListRow(
    Guid RequestId,
    Guid PatientUserId,
    SupervisionRequestState RequestState,
    DateTime RequestedAtUtc,
    DateTime ExpiresAtUtc,
    string? ProfilePhotoStorageKey,
    string FirstName,
    string LastName);
