using DrugsApp.Domain.AuthFeature.Interfaces;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.ReadModels;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.Features.UserProfileFeature.ApplicationServices;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyPatient;

/// <summary>
/// Handles study patient updates through the shared personal-data service of the user profile feature.
/// </summary>
internal sealed class UpdateStudyPatientHandler(
    IStudyClinicReadRepository _studyClinicReadRepository,
    IUserProfileApplicationService _userProfileApplicationService)
    : IRequestHandler<UpdateStudyPatientCommand, ErrorOr<StudyPatientNodeDto>>
{
    /// <summary>
    /// Updates personal data of a patient member and returns the refreshed node.
    /// </summary>
    /// <param name="command">Update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated patient node, not found, or validation errors.</returns>
    public async Task<ErrorOr<StudyPatientNodeDto>> Handle(UpdateStudyPatientCommand command, CancellationToken cancellationToken)
    {
        StudyMemberDetailReadModel? detail = await _studyClinicReadRepository.GetMemberDetailAsync(
            command.ClinicId,
            command.PatientUserId,
            StudyMemberKind.Patient,
            cancellationToken);
        if (detail is null)
        {
            return StudyModeErrors.MemberNotFound(command.PatientUserId, "patientId");
        }

        UserProfilePersonalData personalData = command.Request.MapToPersonalData();

        ErrorOr<UserProfilePersonalDataUpdateResult> updateResult =
            await _userProfileApplicationService.UpdatePersonalDataAsync(
                detail.UserId,
                personalData,
                cancellationToken);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        return personalData.MapToResponse(detail);
    }
}
