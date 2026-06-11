using System;
using System.Collections.Generic;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

public sealed record ProviderPatientListItemDto(
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

public sealed record ProviderPatientListResponse(
    IReadOnlyCollection<ProviderPatientListItemDto> Items,
    int TotalCount,
    bool HasMore);

public sealed record ProviderPatientDetailResponse(
    Guid ConnectionId,
    Guid PatientUserId,
    string AccessLevelCode,
    PatientBasicProfileDto PatientProfile,
    object? WomenSpecificSummary,
    object? RegularDrugsSummary,
    object? RegularSupplementsSummary,
    object? RegularFoodSummary,
    object? AllergiesSummary,
    object? HealthConditionsSummary,
    object? DrugProblemsSummary);

public sealed record PatientBasicProfileDto(
    bool HasProfilePhoto,
    string FirstName,
    string LastName);

public sealed record CancelProviderRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime ResolvedAtUtc);

public sealed record CreateProviderRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime ExpiresAtUtc,
    DateTime RequestedAtUtc);

public sealed record ProposalDraftItemDto(
    string CategoryCode,
    string Source,
    Guid? RelatedCatalogId,
    string? CustomValue,
    string ApplyPayload,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

public sealed record SubmitProviderProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime SubmittedAtUtc,
    string ProposalSummary);

public sealed record ProviderWomenSpecificSummaryDto(
    bool HasProfile,
    bool? CurrentlyPregnant,
    bool? PlanningToGetPregnant,
    bool? Menopause,
    string? ContraceptionMethodCode);

public sealed record ProviderRegularDrugsSummaryDto(
    int TotalCount,
    IReadOnlyCollection<ProviderRegularDrugSummaryItemDto> Items);

public sealed record ProviderRegularDrugSummaryItemDto(
    Guid Id,
    string DrugName,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record ProviderRegularSupplementsSummaryDto(
    int TotalCount,
    IReadOnlyCollection<ProviderRegularSupplementSummaryItemDto> Items);

public sealed record ProviderRegularSupplementSummaryItemDto(
    Guid Id,
    string DisplayName,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record ProviderRegularFoodSummaryDto(
    int TotalCount,
    IReadOnlyCollection<ProviderRegularFoodSummaryItemDto> Items);

public sealed record ProviderRegularFoodSummaryItemDto(
    Guid Id,
    string FoodName,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record ProviderAllergiesSummaryDto(
    ProviderAllergyGeneralSummaryDto? GeneralSummary,
    int DrugAllergyCount,
    int SupplementAllergyCount,
    int FoodAllergyCount,
    int OtherAllergyCount);

public sealed record ProviderAllergyGeneralSummaryDto(
    string? SevereReactionStatus,
    bool? CarriesAllergyMedication,
    string? PollenDustMitesAllergyStatus,
    string? InsectVenomAllergyStatus,
    string? LatexAllergyStatus);

public sealed record ProviderHealthConditionsSummaryDto(
    int TotalCount,
    IReadOnlyCollection<ProviderHealthConditionSummaryItemDto> Items);

public sealed record ProviderHealthConditionSummaryItemDto(
    Guid Id,
    string DisplayName,
    string? Note);

public sealed record ProviderDrugProblemsSummaryDto(
    int TotalCount,
    IReadOnlyCollection<ProviderDrugProblemSummaryItemDto> Items);

public sealed record ProviderDrugProblemSummaryItemDto(
    Guid Id,
    string DisplayName,
    string? ProblemNote);
