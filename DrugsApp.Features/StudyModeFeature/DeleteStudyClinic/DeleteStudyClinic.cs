using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.DeleteStudyClinic;

/// <summary>
/// Handles study clinic deletion: member account teardown followed by the staged clinic removal; any failure
/// rolls back the entire teardown through <c>TransactionBehavior</c>.
/// </summary>
internal sealed class DeleteStudyClinicHandler(
    IStudyClinicRepository _studyClinicRepository,
    IStudyAccountTeardownApplicationService _studyAccountTeardownApplicationService)
    : IRequestHandler<DeleteStudyClinicCommand, ErrorOr<Deleted>>
{
    /// <summary>
    /// Tears down every member account and removes the clinic.
    /// </summary>
    /// <param name="command">Deletion command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted, not found, or teardown errors.</returns>
    public async Task<ErrorOr<Deleted>> Handle(DeleteStudyClinicCommand command, CancellationToken cancellationToken)
    {
        StudyClinic? clinic = await _studyClinicRepository.GetByIdAsync(command.ClinicId, track: true, cancellationToken);
        if (clinic is null)
        {
            return StudyModeErrors.ClinicNotFound(command.ClinicId);
        }

        ErrorOr<Success> teardownResult = await _studyAccountTeardownApplicationService.TeardownAllMembersAsync(
            clinic,
            cancellationToken);
        if (teardownResult.IsError)
        {
            return teardownResult.Errors;
        }

        _studyClinicRepository.Remove(clinic);

        return Result.Deleted;
    }
}
