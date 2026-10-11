using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.DeleteStudyAccount;

/// <summary>
/// Handles single study account deletion through the shared teardown service.
/// </summary>
internal sealed class DeleteStudyAccountHandler(
    IStudyClinicRepository _studyClinicRepository,
    IStudyAccountTeardownApplicationService _studyAccountTeardownApplicationService)
    : IRequestHandler<DeleteStudyAccountCommand, ErrorOr<Deleted>>
{
    /// <summary>
    /// Removes the member from the clinic and tears down its account.
    /// </summary>
    /// <param name="command">Deletion command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted, not found, or teardown errors.</returns>
    public async Task<ErrorOr<Deleted>> Handle(DeleteStudyAccountCommand command, CancellationToken cancellationToken)
    {
        StudyClinic? clinic = await _studyClinicRepository.GetByIdAsync(command.ClinicId, track: true, cancellationToken);
        if (clinic is null)
        {
            return StudyModeErrors.ClinicNotFound(command.ClinicId);
        }

        if (clinic.FindMember(command.UserId, null) is null)
        {
            return StudyModeErrors.MemberNotFound(command.UserId, "userId");
        }

        ErrorOr<Success> teardownResult = await _studyAccountTeardownApplicationService.TeardownMemberAsync(
            clinic,
            command.UserId,
            cancellationToken);
        if (teardownResult.IsError)
        {
            return teardownResult.Errors;
        }

        return Result.Deleted;
    }
}
