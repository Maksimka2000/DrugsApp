using DrugsApp.Domain.PatientSupervisionFeature.Entities;

namespace DrugsApp.Domain.PatientSupervisionFeature.Repositories;

public interface IMedicalChangeProposalRepository
{
    Task<IReadOnlyCollection<MedicalChangeProposalListRow>> SearchForPatientAsync(
        Guid patientUserId,
        Guid? providerUserId,
        string? state,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<MedicalChangeProposalDetail?> GetDetailForPatientAsync(Guid patientUserId, Guid proposalId, CancellationToken cancellationToken);

    Task<MedicalChangeProposalDetail?> GetDetailForParticipantAsync(Guid userId, Guid proposalId, CancellationToken cancellationToken);

    Task<MedicalChangeProposal?> GetTrackedDetailForPatientAsync(Guid patientUserId, Guid proposalId, CancellationToken cancellationToken);

    Task<MedicalChangeProposal?> GetTrackedByIdForPatientAsync(Guid patientUserId, Guid proposalId, CancellationToken cancellationToken);

    Task<int> InvalidatePendingByConnectionAsync(Guid connectionId, DateTime resolvedAtUtc, CancellationToken cancellationToken);

    Task AddAsync(MedicalChangeProposal proposal, CancellationToken cancellationToken);
}

public sealed record MedicalChangeProposalListRow(
    Guid ProposalId,
    Guid HealthcareProfessionalUserId,
    string ProposalState,
    string ProposalSummary,
    DateTime SubmittedAtUtc);

public sealed record MedicalChangeProposalDetail(
    Guid ProposalId,
    Guid SupervisionConnectionId,
    Guid PatientUserId,
    Guid HealthcareProfessionalUserId,
    string ProposalState,
    string ProposalSummary,
    DateTime SubmittedAtUtc,
    IReadOnlyCollection<MedicalChangeProposalSectionDto> Sections);

public sealed record MedicalChangeProposalSectionDto(
    string SectionCode,
    IReadOnlyCollection<MedicalChangeProposalSectionItemDto> Items);

public sealed record MedicalChangeProposalSectionItemDto(
    string CategoryCode,
    string Source,
    Guid? RelatedCatalogId,
    string? CustomValue,
    string DisplayName,
    string? SecondaryText,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Note,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);
