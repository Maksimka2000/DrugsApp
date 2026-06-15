using System;
using System.Collections.Generic;
using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularFoodFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

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
    int? TotalCount,
    bool HasMore,
    string? MatchedFieldToken,
    List<string> MatchedFailedFieldTokens);

public sealed record ProviderPatientProfileDto(
    string? ProfilePhotoUrl,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender);

public sealed record ProviderPatientDetailResponse(
    Guid ConnectionId,
    Guid PatientUserId,
    string AccessLevelCode,
    string? AccessBlockedMessage,
    ProviderPatientProfileDto? PatientProfile,
    ProviderReproductiveHealthDetailDto? ReproductiveHealth,
    HealthConditionsGroupReadModel? HealthConditions,
    ProblematicDrugsGroupReadModel? ProblematicDrugs,
    AllergiesGroupReadModel? Allergies,
    IReadOnlyCollection<RegularDrugListItemDto>? RegularDrugs,
    IReadOnlyCollection<RegularFoodListItemDto>? RegularFoods,
    IReadOnlyCollection<RegularSupplementListItemDto>? RegularSupplements);

public sealed record CreateProviderRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime RequestedAtUtc);

public sealed record CancelProviderSupervisionRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime ResolvedAtUtc);

public sealed record ProposalDraftItemDto(
    string CategoryCode,
    string? Source,
    Guid? RelatedCatalogId,
    string? CustomValue,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Note,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

public sealed record SubmitProviderProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime SubmittedAtUtc,
    string ProposalSummary);

public sealed record ProviderReproductiveHealthDetailDto(
    bool HasProfile,
    bool? CurrentlyPregnant,
    bool? PlanningToGetPregnant,
    bool? PregnancyPlanningUnknown,
    bool? Menopause,
    string? ContraceptionMethodCode);
