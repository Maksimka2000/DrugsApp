using System;
using System.Collections.Generic;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.DrugProblemsFeature.Shared;
using DrugsApp.Features.HealthConditionFeature.Shared;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using DrugsApp.Features.RegularDrugsFeature.Shared;
using DrugsApp.Features.RegularFoodFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.Shared;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;

/// <summary>List item describing a provider's patient.</summary>
/// <param name="EntryType">Type of entry (request/connection).</param>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="RequestId">Associated request id if any.</param>
/// <param name="ConnectionId">Associated connection id if any.</param>
/// <param name="RelationshipState">Current relationship state.</param>
/// <param name="AccessLevelCode">Access level code.</param>
/// <param name="FirstName">Patient first name.</param>
/// <param name="LastName">Patient last name.</param>
/// <param name="HasProfilePhoto">Whether a profile photo exists.</param>
/// <param name="RequestedAtUtc">Request timestamp.</param>
/// <param name="ApprovedAtUtc">Approval timestamp.</param>
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

/// <summary>Response for listing provider patients.</summary>
/// <param name="Items">List items.</param>
/// <param name="TotalCount">Optional total count.</param>
/// <param name="HasMore">Whether more pages exist.</param>
/// <param name="MatchedFieldToken">Matched field token.</param>
/// <param name="MatchedFailedFieldTokens">Tokens that did not match.</param>
public sealed record ProviderPatientListResponse(
    IReadOnlyCollection<ProviderPatientListItemDto> Items,
    int? TotalCount,
    bool HasMore,
    string? MatchedFieldToken,
    List<string> MatchedFailedFieldTokens);

/// <summary>Basic profile information for a patient.</summary>
/// <param name="ProfilePhotoUrl">Profile photo URL.</param>
/// <param name="FirstName">First name.</param>
/// <param name="LastName">Last name.</param>
/// <param name="DateOfBirth">Date of birth.</param>
/// <param name="Gender">Gender string.</param>
public sealed record ProviderPatientProfileDto(
    string? ProfilePhotoUrl,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender);

/// <summary>Detailed view of a provider's patient.</summary>
/// <param name="ConnectionId">Supervision connection id.</param>
/// <param name="PatientUserId">Patient user id.</param>
/// <param name="AccessLevelCode">Access level code.</param>
/// <param name="AccessBlockedMessage">Optional access blocked message.</param>
/// <param name="PatientProfile">Patient profile data.</param>
/// <param name="ReproductiveHealth">Reproductive health details.</param>
/// <param name="AllergySummary">Allergy summary answers (reaction, pollen, insects, latex).</param>
/// <param name="HealthConditions">Health conditions in the member list shape.</param>
/// <param name="DrugProblems">Drug problems in the member list shape.</param>
/// <param name="DrugAllergies">Drug allergies in the member list shape.</param>
/// <param name="SupplementAllergies">Supplement allergies in the member list shape.</param>
/// <param name="FoodAllergies">Food allergies in the member list shape.</param>
/// <param name="OtherAllergies">Other allergies in the member list shape.</param>
/// <param name="RegularDrugs">Live regular drugs list.</param>
/// <param name="RegularFoods">Live regular foods list.</param>
/// <param name="RegularSupplements">Live regular supplements list.</param>
/// <param name="RegularDrugHistory">Latest archived regular drugs.</param>
/// <param name="RegularFoodHistory">Latest archived regular foods.</param>
/// <param name="RegularSupplementHistory">Latest archived regular supplements.</param>
/// <param name="MealTimes">Patient meal times.</param>
public sealed record ProviderPatientDetailResponse(
    Guid ConnectionId,
    Guid PatientUserId,
    string AccessLevelCode,
    string? AccessBlockedMessage,
    ProviderPatientProfileDto? PatientProfile,
    ProviderReproductiveHealthDetailDto? ReproductiveHealth,
    AllergySummaryResponse? AllergySummary,
    IReadOnlyCollection<UserHealthConditionDto>? HealthConditions,
    IReadOnlyCollection<DrugProblemListItemDto>? DrugProblems,
    IReadOnlyCollection<ClientDrugAllergyDto>? DrugAllergies,
    IReadOnlyCollection<ClientSupplementAllergyDto>? SupplementAllergies,
    IReadOnlyCollection<ClientFoodAllergyDto>? FoodAllergies,
    IReadOnlyCollection<ClientOtherAllergyDto>? OtherAllergies,
    IReadOnlyCollection<RegularDrugListItemDto>? RegularDrugs,
    IReadOnlyCollection<RegularFoodListItemDto>? RegularFoods,
    IReadOnlyCollection<RegularSupplementListItemDto>? RegularSupplements,
    ProviderIntakeHistoryDto<RegularDrugHistoryItemDto>? RegularDrugHistory,
    ProviderIntakeHistoryDto<RegularFoodHistoryListItemDto>? RegularFoodHistory,
    ProviderIntakeHistoryDto<RegularSupplementHistoryItemDto>? RegularSupplementHistory,
    IReadOnlyCollection<MealTimeListItemDto>? MealTimes);

/// <summary>Latest archived intake rows of one kind for a provider view.</summary>
/// <typeparam name="T">History item type.</typeparam>
/// <param name="Items">Archived rows, newest first.</param>
/// <param name="HasMore">Whether older archived rows exist beyond the returned page.</param>
public sealed record ProviderIntakeHistoryDto<T>(
    IReadOnlyCollection<T> Items,
    bool HasMore);

/// <summary>Response after creating a provider request.</summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="RequestState">Request state.</param>
/// <param name="RequestedAtUtc">Timestamp of request.</param>
public sealed record CreateProviderRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime RequestedAtUtc);

/// <summary>Response after canceling a provider supervision request.</summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="RequestState">Updated request state.</param>
/// <param name="ResolvedAtUtc">Resolution timestamp.</param>
public sealed record CancelProviderSupervisionRequestResponseDto(
    Guid RequestId,
    string RequestState,
    DateTime ResolvedAtUtc);

/// <summary>List item for provider supervision requests.</summary>
/// <param name="RequestId">Request identifier.</param>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="RequestState">Current request state.</param>
/// <param name="RequestedAtUtc">Request timestamp.</param>
/// <param name="ExpiresAtUtc">Expiration timestamp.</param>
/// <param name="FirstName">Patient first name.</param>
/// <param name="LastName">Patient last name.</param>
/// <param name="ProfilePhotoUrl">Profile photo URL.</param>
public sealed record ProviderSupervisionRequestListItemDto(
    Guid RequestId,
    Guid PatientUserId,
    string RequestState,
    DateTime RequestedAtUtc,
    DateTime ExpiresAtUtc,
    string FirstName,
    string LastName,
    string? ProfilePhotoUrl);

/// <summary>Response for listing provider supervision requests.</summary>
/// <param name="Items">List items.</param>
/// <param name="TotalCount">Optional total count.</param>
/// <param name="HasMore">Whether more pages exist.</param>
public sealed record ProviderSupervisionRequestListResponse(
    IReadOnlyCollection<ProviderSupervisionRequestListItemDto> Items,
    int? TotalCount,
    bool HasMore);

/// <summary>Draft item for provider-submitted proposals.</summary>
/// <param name="CategoryCode">Category code.</param>
/// <param name="Source">Source token.</param>
/// <param name="RelatedCatalogId">Related catalog id when applicable.</param>
/// <param name="CustomValue">Custom value text.</param>
/// <param name="StartDate">Start date.</param>
/// <param name="EndDate">End date.</param>
/// <param name="Note">Optional note.</param>
/// <param name="IntakeTimes">Intake times.</param>
public sealed record ProposalDraftItemDto(
    string CategoryCode,
    string? Source,
    Guid? RelatedCatalogId,
    string? CustomValue,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Note,
    IReadOnlyCollection<TimeOnly>? IntakeTimes);

/// <summary>Response after submitting a provider proposal.</summary>
/// <param name="ProposalId">Proposal identifier.</param>
/// <param name="ProposalState">Current proposal state.</param>
/// <param name="SubmittedAtUtc">Submission timestamp.</param>
/// <param name="ProposalSummary">Proposal summary text.</param>
public sealed record SubmitProviderProposalResponseDto(
    Guid ProposalId,
    string ProposalState,
    DateTime SubmittedAtUtc,
    string ProposalSummary);

/// <summary>Reproductive health detail for provider view.</summary>
/// <param name="HasProfile">Whether a profile exists.</param>
/// <param name="CurrentlyPregnant">Pregnancy status.</param>
/// <param name="PlanningToGetPregnant">Planning to get pregnant flag.</param>
/// <param name="PregnancyPlanningUnknown">Unknown planning status.</param>
/// <param name="Menopause">Menopause flag.</param>
/// <param name="ContraceptionMethodCode">Contraception method code.</param>
public sealed record ProviderReproductiveHealthDetailDto(
    bool HasProfile,
    bool? CurrentlyPregnant,
    bool? PlanningToGetPregnant,
    bool? PregnancyPlanningUnknown,
    bool? Menopause,
    string? ContraceptionMethodCode);
