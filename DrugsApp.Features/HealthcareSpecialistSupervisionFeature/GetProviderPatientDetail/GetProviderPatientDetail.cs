using DrugsApp.Domain.PatientSupervisionFeature;
using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;

using DrugsApp.Domain.UserProfileFeature.Entities;

using DrugsApp.Domain.UserProfileFeature.Repositories;

using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ApplicationServices;

using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using DrugsApp.Features.PatientSupervisionFeature.Shared;

using ErrorOr;

using MediatR;



namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail;



/// <summary>
/// Request payload to retrieve a patient's details for a provider.
/// </summary>
/// <param name="PatientId">Patient user identifier.</param>
public sealed record GetProviderPatientDetailRequest(Guid PatientId);

/// <summary>
/// Query to retrieve patient details for a provider.
/// </summary>
/// <param name="ProviderUserId">Provider user identifier.</param>
/// <param name="PatientId">Patient user identifier.</param>
public sealed record GetProviderPatientDetailQuery(Guid ProviderUserId, Guid PatientId)
    : IRequest<ErrorOr<ProviderPatientDetailResponse>>;



internal sealed class GetProviderPatientDetailHandler(

    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,

    ISupervisionConnectionRepository supervisionConnectionRepository,

    IUserProfileRepository userProfileRepository,

    IPatientMedicalProfileReadService medicalProfileReadService)

    : IRequestHandler<GetProviderPatientDetailQuery, ErrorOr<ProviderPatientDetailResponse>>

{

    public async Task<ErrorOr<ProviderPatientDetailResponse>> Handle(

        GetProviderPatientDetailQuery query,

        CancellationToken cancellationToken)

    {

        ErrorOr<Success> profileGuard = await HealthcareProviderProfileGuard.EnsureExistsAsync(

            healthcareProviderProfileRepository,

            query.ProviderUserId,

            cancellationToken);

        if (profileGuard.IsError)

        {

            return profileGuard.Errors;

        }



        SupervisionConnection? connection = await supervisionConnectionRepository.GetActiveForProviderAndPatientAsync(

            query.ProviderUserId,

            query.PatientId,

            track: false,

            cancellationToken);



        if (connection is null)

        {

            return Error.NotFound(description: "Active supervision connection not found for patient.");

        }



        if (connection.AccessLevelCode == SupervisionAccessLevel.BlockAccess)
        {
            return new ProviderPatientDetailResponse(
                ConnectionId: connection.Id,
                PatientUserId: connection.PatientUserId,
                AccessLevelCode: SupervisionAccessLevelCatalog.ToApiCode(connection.AccessLevelCode),
                AccessBlockedMessage: "Access is blocked by the patient.",
                PatientProfile: null,
                ReproductiveHealth: null,
                AllergySummary: null,
                HealthConditions: null,
                DrugProblems: null,
                DrugAllergies: null,
                SupplementAllergies: null,
                FoodAllergies: null,
                OtherAllergies: null,
                RegularDrugs: null,
                RegularFoods: null,
                RegularSupplements: null,
                RegularDrugHistory: null,
                RegularFoodHistory: null,
                RegularSupplementHistory: null,
                MealTimes: null);
        }

        UserProfile? patientProfile = await userProfileRepository.GetByUserIdAsync(connection.PatientUserId, cancellationToken);

        if (patientProfile is null)

        {

            return Error.NotFound(description: "Patient profile not found.");

        }



        var patientProfileDto = new ProviderPatientProfileDto(
            ProfilePhotoUrl: MemberProfilePhotoUrls.Build(
                connection.PatientUserId,
                patientProfile.ProfilePhotoStorageKey),
            FirstName: patientProfile.FirstName,
            LastName: patientProfile.LastName,
            DateOfBirth: patientProfile.DateOfBirth,
            Gender: patientProfile.Gender.ToString());



        if (connection.AccessLevelCode == SupervisionAccessLevel.RestrictedView)

        {

            return new ProviderPatientDetailResponse(
                ConnectionId: connection.Id,
                PatientUserId: connection.PatientUserId,
                AccessLevelCode: SupervisionAccessLevelCatalog.ToApiCode(connection.AccessLevelCode),
                AccessBlockedMessage: null,
                PatientProfile: patientProfileDto,
                ReproductiveHealth: null,
                AllergySummary: null,
                HealthConditions: null,
                DrugProblems: null,
                DrugAllergies: null,
                SupplementAllergies: null,
                FoodAllergies: null,
                OtherAllergies: null,
                RegularDrugs: null,
                RegularFoods: null,
                RegularSupplements: null,
                RegularDrugHistory: null,
                RegularFoodHistory: null,
                RegularSupplementHistory: null,
                MealTimes: null);
        }

        ProviderPatientMedicalDetailData medicalProfile = await medicalProfileReadService.LoadAsync(
            connection.PatientUserId,
            cancellationToken);

        return new ProviderPatientDetailResponse(
            ConnectionId: connection.Id,
            PatientUserId: connection.PatientUserId,
            AccessLevelCode: SupervisionAccessLevelCatalog.ToApiCode(connection.AccessLevelCode),
            AccessBlockedMessage: null,
            PatientProfile: patientProfileDto,
            ReproductiveHealth: medicalProfile.ReproductiveHealth,
            AllergySummary: medicalProfile.AllergySummary,
            HealthConditions: medicalProfile.HealthConditions,
            DrugProblems: medicalProfile.DrugProblems,
            DrugAllergies: medicalProfile.DrugAllergies,
            SupplementAllergies: medicalProfile.SupplementAllergies,
            FoodAllergies: medicalProfile.FoodAllergies,
            OtherAllergies: medicalProfile.OtherAllergies,
            RegularDrugs: medicalProfile.RegularDrugs,
            RegularFoods: medicalProfile.RegularFoods,
            RegularSupplements: medicalProfile.RegularSupplements,
            RegularDrugHistory: medicalProfile.RegularDrugHistory,
            RegularFoodHistory: medicalProfile.RegularFoodHistory,
            RegularSupplementHistory: medicalProfile.RegularSupplementHistory,
            MealTimes: medicalProfile.MealTimes);

    }

}


