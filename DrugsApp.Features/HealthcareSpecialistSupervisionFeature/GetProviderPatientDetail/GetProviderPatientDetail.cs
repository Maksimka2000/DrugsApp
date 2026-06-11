using DrugsApp.Domain.PatientSupervisionFeature.Entities;
using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.PatientSupervisionFeature.Interfaces;
using DrugsApp.Domain.PatientSupervisionFeature.Repositories;
using DrugsApp.Domain.PatientSupervisionFeature.ValueObjects;
using DrugsApp.Domain.UserProfileFeature.Entities;
using DrugsApp.Domain.UserProfileFeature.Repositories;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail;

public sealed record GetProviderPatientDetailRequest(Guid ConnectionId);

public sealed record GetProviderPatientDetailQuery(Guid ProviderUserId, Guid ConnectionId)
    : IRequest<ErrorOr<ProviderPatientDetailResponse>>;

internal sealed class GetProviderPatientDetailHandler(
    IHealthcareProviderProfileRepository healthcareProviderProfileRepository,
    ISupervisionConnectionRepository supervisionConnectionRepository,
    IUserProfileRepository userProfileRepository,
    IProviderPatientMedicalSummaryReadService medicalSummaryReadService)
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

        SupervisionConnection? connection = await supervisionConnectionRepository.GetByIdForProviderAsync(
            query.ProviderUserId,
            query.ConnectionId,
            track: false,
            cancellationToken);

        if (connection is null || connection.ConnectionState != SupervisionConnectionState.Active)
        {
            return Error.NotFound(description: "Connection not found or inactive.");
        }

        if (connection.AccessLevelCode == SupervisionAccessLevel.BlockAccess)
        {
            return Error.Forbidden(description: "Access is blocked.");
        }

        UserProfile? patientProfile = await userProfileRepository.GetByUserIdAsync(connection.PatientUserId, cancellationToken);
        if (patientProfile is null)
        {
            return Error.NotFound(description: "Patient profile not found.");
        }

        var basicProfile = new PatientBasicProfileDto(
            HasProfilePhoto: patientProfile.ProfilePhotoStorageKey is not null,
            FirstName: patientProfile.FirstName,
            LastName: patientProfile.LastName);

        if (connection.AccessLevelCode == SupervisionAccessLevel.RestrictedView)
        {
            return new ProviderPatientDetailResponse(
                ConnectionId: connection.Id,
                PatientUserId: connection.PatientUserId,
                AccessLevelCode: connection.AccessLevelCode.ToString(),
                PatientProfile: basicProfile,
                WomenSpecificSummary: null,
                RegularDrugsSummary: null,
                RegularSupplementsSummary: null,
                RegularFoodSummary: null,
                AllergiesSummary: null,
                HealthConditionsSummary: null,
                DrugProblemsSummary: null);
        }

        ProviderPatientMedicalSummaries summaries = await medicalSummaryReadService.LoadSummariesAsync(
            connection.PatientUserId,
            cancellationToken);

        return new ProviderPatientDetailResponse(
            ConnectionId: connection.Id,
            PatientUserId: connection.PatientUserId,
            AccessLevelCode: connection.AccessLevelCode.ToString(),
            PatientProfile: basicProfile,
            WomenSpecificSummary: summaries.WomenSpecificSummary,
            RegularDrugsSummary: summaries.RegularDrugsSummary,
            RegularSupplementsSummary: summaries.RegularSupplementsSummary,
            RegularFoodSummary: summaries.RegularFoodSummary,
            AllergiesSummary: summaries.AllergiesSummary,
            HealthConditionsSummary: summaries.HealthConditionsSummary,
            DrugProblemsSummary: summaries.DrugProblemsSummary);
    }
}
