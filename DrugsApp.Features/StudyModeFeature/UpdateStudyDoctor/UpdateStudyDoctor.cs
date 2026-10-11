using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyDoctor;

/// <summary>
/// Handles study doctor updates through the user profile feature's services.
/// </summary>
internal sealed class UpdateStudyDoctorHandler(
    IStudyClinicRepository _studyClinicRepository,
    IUserProfileApplicationService _userProfileApplicationService,
    IHealthcareProviderProfileApplicationService _healthcareProviderProfileApplicationService)
    : IRequestHandler<UpdateStudyDoctorCommand, ErrorOr<StudyDoctorNodeDto>>
{
    /// <summary>
    /// Updates personal data and the provider profile of a doctor member.
    /// </summary>
    /// <param name="command">Update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated doctor node, not found, or validation errors.</returns>
    public async Task<ErrorOr<StudyDoctorNodeDto>> Handle(UpdateStudyDoctorCommand command, CancellationToken cancellationToken)
    {
        StudyClinicMember? member = await _studyClinicRepository.GetMemberAsync(
            command.ClinicId,
            command.DoctorUserId,
            cancellationToken);
        if (member is null || member.MemberKind != StudyMemberKind.Doctor)
        {
            return StudyModeErrors.MemberNotFound(command.DoctorUserId, "doctorId");
        }

        ErrorOr<UserProfilePersonalDataUpdateResult> personalDataResult =
            await _userProfileApplicationService.UpdatePersonalDataAsync(
                member.UserId,
                command.Request.MapToPersonalData(),
                cancellationToken);
        if (personalDataResult.IsError)
        {
            return personalDataResult.Errors;
        }

        ErrorOr<HealthcareProviderProfileUpsertResult> providerResult =
            await _healthcareProviderProfileApplicationService.UpsertFromOnboardingAsync(
                member.UserId,
                command.Request.MapToProviderRequest(),
                cancellationToken);
        if (providerResult.IsError)
        {
            return providerResult.Errors;
        }

        return command.Request.MapToResponse(member);
    }
}
