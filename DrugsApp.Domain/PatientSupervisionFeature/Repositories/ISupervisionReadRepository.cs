using DrugsApp.Domain.PatientSupervisionFeature.Entities;

namespace DrugsApp.Domain.PatientSupervisionFeature.Repositories;

public interface ISupervisionReadRepository
{
    Task<SearchPatientHealthcareProfessionalsResult> SearchPatientHealthcareProfessionalsAsync(
        Guid patientUserId,
        string? relationshipState,
        string? sortBy,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<PatientHealthcareProfessionalDetail?> GetPatientHealthcareProfessionalDetailAsync(
        Guid patientUserId,
        Guid healthcareProfessionalUserId,
        CancellationToken cancellationToken);

    Task<SearchProviderPatientsResult> SearchProviderPatientsAsync(
        Guid providerUserId,
        string? relationshipState,
        string? sortBy,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken);
}

public sealed record PatientHealthcareProfessionalListRow(
    string EntryType,
    Guid HealthcareProfessionalUserId,
    Guid? RequestId,
    Guid? ConnectionId,
    string RelationshipState,
    string? AccessLevelCode,
    string FirstName,
    string LastName,
    string? ProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    bool HasPendingProposal,
    int? PendingProposalCount,
    DateTime RequestedAtUtc,
    DateTime? ApprovedAtUtc,
    bool HasProfilePhoto);

public sealed record PatientHealthcareProfessionalDetail(
    Guid HealthcareProfessionalUserId,
    bool HasProfilePhoto,
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
    string FirstName,
    string LastName,
    bool HasProfilePhoto,
    DateTime RequestedAtUtc,
    DateTime? ApprovedAtUtc);

public sealed record SearchProviderPatientsResult(
    IReadOnlyCollection<ProviderPatientListRow> Items,
    int TotalCount);
