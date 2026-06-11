using System;
using System.Collections.Generic;

namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

public sealed record PatientHealthcareProfessionalListItemDto(
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

public sealed record PatientHealthcareProfessionalListResponse(
    IReadOnlyCollection<PatientHealthcareProfessionalListItemDto> Items,
    int TotalCount,
    bool HasMore);

public sealed record PatientHealthcareProfessionalDetailDto(
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

public sealed record SupervisionRequestDetailDto(
    Guid RequestId,
    string RequestState,
    DateTime ExpiresAtUtc,
    string HealthcareProfessionalSummary,
    IReadOnlyList<string> AvailableAccessLevels,
    string PrivacyWarningText);

public sealed record RemoveSupervisionConnectionResponseDto(
    Guid ConnectionId,
    string ConnectionState,
    DateTime RemovedAtUtc,
    int? InvalidatedProposalCount);

public sealed record PatientProposalListItemDto(
    Guid ProposalId,
    Guid HealthcareProfessionalUserId,
    string ProposalState,
    string ProposalSummary,
    DateTime SubmittedAtUtc);

public sealed record PatientProposalListResponse(
    IReadOnlyCollection<PatientProposalListItemDto> Items,
    int TotalCount,
    bool HasMore);

public sealed record PatientProposalDetailSectionItemDto(
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

public sealed record PatientProposalDetailSectionDto(
    string SectionCode,
    IReadOnlyCollection<PatientProposalDetailSectionItemDto> Items);

public sealed record PatientProposalDetailResponse(
    Guid ProposalId,
    string ProposalState,
    string ProposalSummary,
    DateTime SubmittedAtUtc,
    string HealthcareProfessionalSummary,
    IReadOnlyCollection<PatientProposalDetailSectionDto> Sections);

public sealed record ApproveSupervisionRequestResponseDto(
    Guid RequestId,
    string RequestState,
    Guid ConnectionId,
    string AccessLevelCode,
    DateTime ApprovedAtUtc);

public sealed record RejectSupervisionRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime ResolvedAtUtc);

public sealed record UpdateSupervisionAccessLevelResponseDto(
    Guid ConnectionId,
    string AccessLevelCode,
    DateTime UpdatedAtUtc);

public sealed record ApprovePatientProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime ResolvedAtUtc,
    IReadOnlyDictionary<string, int> CreatedRecordCounts,
    string? InvalidatedReason);

public sealed record RejectPatientProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime ResolvedAtUtc);
