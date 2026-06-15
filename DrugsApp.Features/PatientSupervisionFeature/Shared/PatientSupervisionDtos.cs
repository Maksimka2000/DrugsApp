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
    string? ProfilePhotoUrl);

public sealed record PatientHealthcareProfessionalListResponse(
    IReadOnlyCollection<PatientHealthcareProfessionalListItemDto> Items,
    int? TotalCount,
    bool HasMore,
    string? MatchedFieldToken,
    List<string> MatchedFailedFieldTokens);

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

public sealed record PatientSupervisionRequestListItemDto(
    Guid RequestId,
    Guid HealthcareProfessionalUserId,
    string RequestState,
    DateTime RequestedAtUtc,
    string FirstName,
    string LastName,
    string? ProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    string? ProfilePhotoUrl);

public sealed record PatientSupervisionRequestListResponse(
    IReadOnlyCollection<PatientSupervisionRequestListItemDto> Items,
    int? TotalCount,
    bool HasMore);

public sealed record SupervisionRequestHealthcareProfessionalDto(
    Guid HealthcareProfessionalUserId,
    string? ProfilePhotoUrl,
    string FirstName,
    string LastName,
    string? ProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription);

public sealed record SupervisionRequestDetailDto(
    Guid RequestId,
    string RequestState,
    DateTime RequestedAtUtc,
    SupervisionRequestHealthcareProfessionalDto HealthcareProfessional);

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

public sealed record PatientProposalDraftItemDto(
    string CategoryCode,
    string? Source,
    Guid? RelatedCatalogId,
    string? CustomValue,
    string DisplayName,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Note,
    IReadOnlyCollection<TimeOnly> IntakeTimes,
    string ApplyPayload);

public sealed record PatientProposalDetailResponse(
    Guid ProposalId,
    string ProposalState,
    string ProposalSummary,
    DateTime SubmittedAtUtc,
    SupervisionRequestHealthcareProfessionalDto HealthcareProfessional,
    IReadOnlyCollection<PatientProposalDraftItemDto> DraftItems);

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

public sealed record ProposalCreatedRecordCountsDto(
    int RegularDrug,
    int RegularSupplement,
    int RegularFood);

public sealed record ApprovePatientProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime ResolvedAtUtc,
    ProposalCreatedRecordCountsDto CreatedRecordCounts,
    string? InvalidatedReason);

public sealed record RejectPatientProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime ResolvedAtUtc);
