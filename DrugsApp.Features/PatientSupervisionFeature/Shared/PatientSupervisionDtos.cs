using System;
using System.Collections.Generic;

namespace DrugsApp.Features.PatientSupervisionFeature.Shared;

/// <summary>List item describing a healthcare professional related to a patient.</summary>
/// <param name="EntryType">Type of entry (request/connection).</param>
/// <param name="HealthcareProfessionalUserId">Healthcare professional user id.</param>
/// <param name="RequestId">Associated request id if any.</param>
/// <param name="ConnectionId">Associated connection id if any.</param>
/// <param name="RelationshipState">Current relationship state.</param>
/// <param name="AccessLevelCode">Access level code.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="ProfessionLabel">Profession label.</param>
/// <param name="WorkplaceLabel">Workplace label.</param>
/// <param name="ShortDescription">Short description.</param>
/// <param name="HasPendingProposal">Whether pending proposals exist.</param>
/// <param name="PendingProposalCount">Pending proposal count.</param>
/// <param name="RequestedAtUtc">Request timestamp.</param>
/// <param name="ApprovedAtUtc">Approval timestamp.</param>
/// <param name="ProfilePhotoUrl">Profile photo URL.</param>
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

/// <summary>Response for listing patient healthcare professionals.</summary>
/// <param name="Items">List items.</param>
/// <param name="TotalCount">Total count when available.</param>
/// <param name="HasMore">Indicates more pages exist.</param>
/// <param name="MatchedFieldToken">Matched field token.</param>
/// <param name="MatchedFailedFieldTokens">Tokens of fields that failed match.</param>
public sealed record PatientHealthcareProfessionalListResponse(
    IReadOnlyCollection<PatientHealthcareProfessionalListItemDto> Items,
    int? TotalCount,
    bool HasMore,
    string? MatchedFieldToken,
    List<string> MatchedFailedFieldTokens);

/// <summary>Detail DTO for a healthcare professional relationship.</summary>
/// <param name="HealthcareProfessionalUserId">Healthcare professional user id.</param>
/// <param name="HasProfilePhoto">Whether a profile photo exists.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="ProfessionLabel">Profession label.</param>
/// <param name="WorkplaceLabel">Workplace label.</param>
/// <param name="ShortDescription">Short description.</param>
/// <param name="RelationshipState">Relationship state.</param>
/// <param name="AccessLevelCode">Access level code.</param>
/// <param name="HasPendingProposal">Whether pending proposals exist.</param>
/// <param name="PendingProposalCount">Pending proposal count.</param>
/// <param name="ActiveRequestId">Active request id.</param>
/// <param name="ActiveConnectionId">Active connection id.</param>
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

/// <summary>List item for a patient supervision request.</summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="HealthcareProfessionalUserId">Healthcare professional user id.</param>
/// <param name="RequestState">Current request state.</param>
/// <param name="RequestedAtUtc">Request timestamp.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="ProfessionLabel">Profession label.</param>
/// <param name="WorkplaceLabel">Workplace label.</param>
/// <param name="ShortDescription">Short description.</param>
/// <param name="ProfilePhotoUrl">Profile photo URL.</param>
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

/// <summary>Response for patient supervision requests list.</summary>
/// <param name="Items">List items.</param>
/// <param name="TotalCount">Total count when available.</param>
/// <param name="HasMore">Indicates more pages exist.</param>
public sealed record PatientSupervisionRequestListResponse(
    IReadOnlyCollection<PatientSupervisionRequestListItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>Summary of a healthcare professional on a request.</summary>
/// <param name="HealthcareProfessionalUserId">Healthcare professional user id.</param>
/// <param name="ProfilePhotoUrl">Profile photo URL.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="ProfessionLabel">Profession label.</param>
/// <param name="WorkplaceLabel">Workplace label.</param>
/// <param name="ShortDescription">Short description.</param>
public sealed record SupervisionRequestHealthcareProfessionalDto(
    Guid HealthcareProfessionalUserId,
    string? ProfilePhotoUrl,
    string FirstName,
    string LastName,
    string? ProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription);

/// <summary>Detail for a supervision request.</summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="RequestState">Current state.</param>
/// <param name="RequestedAtUtc">Request timestamp.</param>
/// <param name="HealthcareProfessional">Healthcare professional summary.</param>
public sealed record SupervisionRequestDetailDto(
    Guid RequestId,
    string RequestState,
    DateTime RequestedAtUtc,
    SupervisionRequestHealthcareProfessionalDto HealthcareProfessional);

/// <summary>Response for removing a supervision connection.</summary>
/// <param name="ConnectionId">Connection identifier.</param>
/// <param name="ConnectionState">Updated connection state.</param>
/// <param name="RemovedAtUtc">Removal timestamp.</param>
/// <param name="InvalidatedProposalCount">Count of invalidated proposals.</param>
public sealed record RemoveSupervisionConnectionResponseDto(
    Guid ConnectionId,
    string ConnectionState,
    DateTime RemovedAtUtc,
    int? InvalidatedProposalCount);

/// <summary>List item for patient proposals.</summary>
/// <param name="ProposalId">Proposal identifier.</param>
/// <param name="HealthcareProfessionalUserId">Healthcare professional user id.</param>
/// <param name="ProposalState">Current proposal state.</param>
/// <param name="ProposalSummary">Proposal summary.</param>
/// <param name="SubmittedAtUtc">Submission timestamp.</param>
public sealed record PatientProposalListItemDto(
    Guid ProposalId,
    Guid HealthcareProfessionalUserId,
    string ProposalState,
    string ProposalSummary,
    DateTime SubmittedAtUtc);

/// <summary>Response for patient proposal list.</summary>
/// <param name="Items">List items.</param>
/// <param name="TotalCount">Total count.</param>
/// <param name="HasMore">Indicates more pages exist.</param>
public sealed record PatientProposalListResponse(
    IReadOnlyCollection<PatientProposalListItemDto> Items,
    int TotalCount,
    bool HasMore);

/// <summary>Draft item within a patient proposal.</summary>
/// <param name="CategoryCode">Category code.</param>
/// <param name="Source">Source token.</param>
/// <param name="RelatedCatalogId">Related catalog id when applicable.</param>
/// <param name="CustomValue">Custom value text.</param>
/// <param name="DisplayName">Display name.</param>
/// <param name="StartDate">Start date.</param>
/// <param name="EndDate">End date.</param>
/// <param name="Note">Optional note.</param>
/// <param name="IntakeTimes">Intake times.</param>
/// <param name="ApplyPayload">Serialized apply payload.</param>
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

/// <summary>Detail response for a patient proposal.</summary>
/// <param name="ProposalId">Proposal identifier.</param>
/// <param name="ProposalState">Current state.</param>
/// <param name="ProposalSummary">Summary text.</param>
/// <param name="SubmittedAtUtc">Submission timestamp.</param>
/// <param name="HealthcareProfessional">Healthcare professional summary.</param>
/// <param name="DraftItems">Draft items.</param>
public sealed record PatientProposalDetailResponse(
    Guid ProposalId,
    string ProposalState,
    string ProposalSummary,
    DateTime SubmittedAtUtc,
    SupervisionRequestHealthcareProfessionalDto HealthcareProfessional,
    IReadOnlyCollection<PatientProposalDraftItemDto> DraftItems);

/// <summary>Response for approving a supervision request.</summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="RequestState">Updated request state.</param>
/// <param name="ConnectionId">Created connection id.</param>
/// <param name="AccessLevelCode">Assigned access level code.</param>
/// <param name="ApprovedAtUtc">Approval timestamp.</param>
public sealed record ApproveSupervisionRequestResponseDto(
    Guid RequestId,
    string RequestState,
    Guid ConnectionId,
    string AccessLevelCode,
    DateTime ApprovedAtUtc);

/// <summary>Response for rejecting a supervision request.</summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="RequestState">Updated state.</param>
/// <param name="ResolvedAtUtc">Resolution timestamp.</param>
public sealed record RejectSupervisionRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime ResolvedAtUtc);

/// <summary>Response for updating supervision access level.</summary>
/// <param name="ConnectionId">Connection identifier.</param>
/// <param name="AccessLevelCode">New access level code.</param>
/// <param name="UpdatedAtUtc">Update timestamp.</param>
public sealed record UpdateSupervisionAccessLevelResponseDto(
    Guid ConnectionId,
    string AccessLevelCode,
    DateTime UpdatedAtUtc);

/// <summary>Created record counts after applying a proposal.</summary>
/// <param name="RegularDrug">Count of created regular drugs.</param>
/// <param name="RegularSupplement">Count of created regular supplements.</param>
/// <param name="RegularFood">Count of created regular foods.</param>
public sealed record ProposalCreatedRecordCountsDto(
    int RegularDrug,
    int RegularSupplement,
    int RegularFood);

/// <summary>Response for approving a patient proposal.</summary>
/// <param name="ProposalId">Proposal identifier.</param>
/// <param name="ProposalState">Updated state.</param>
/// <param name="ResolvedAtUtc">Resolution timestamp.</param>
/// <param name="CreatedRecordCounts">Counts of created records.</param>
/// <param name="InvalidatedReason">Optional invalidation reason.</param>
public sealed record ApprovePatientProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime ResolvedAtUtc,
    ProposalCreatedRecordCountsDto CreatedRecordCounts,
    string? InvalidatedReason);

/// <summary>Response for rejecting a patient proposal.</summary>
/// <param name="ProposalId">Proposal identifier.</param>
/// <param name="ProposalState">Updated state.</param>
/// <param name="ResolvedAtUtc">Resolution timestamp.</param>
public sealed record RejectPatientProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime ResolvedAtUtc);
