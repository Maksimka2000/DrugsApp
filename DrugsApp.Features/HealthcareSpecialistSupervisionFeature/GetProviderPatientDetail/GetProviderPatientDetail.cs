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



public sealed record GetProviderPatientDetailRequest(Guid PatientId);



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
                HealthConditions: null,
                ProblematicDrugs: null,
                Allergies: null,
                RegularDrugs: null,
                RegularFoods: null,
                RegularSupplements: null);
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
                HealthConditions: null,
                ProblematicDrugs: null,
                Allergies: null,
                RegularDrugs: null,
                RegularFoods: null,
                RegularSupplements: null);
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
            HealthConditions: medicalProfile.HealthConditions,
            ProblematicDrugs: medicalProfile.ProblematicDrugs,
            Allergies: medicalProfile.Allergies,
            RegularDrugs: medicalProfile.RegularDrugs,
            RegularFoods: medicalProfile.RegularFoods,
            RegularSupplements: medicalProfile.RegularSupplements);

    }

}


