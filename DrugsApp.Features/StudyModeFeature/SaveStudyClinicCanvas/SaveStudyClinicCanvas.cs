using DrugsApp.Domain.PatientSupervisionFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.PatientSupervisionFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.SaveStudyClinicCanvas;

/// <summary>
/// Handles canvas saving: association party validation, position updates and replacement of the clinic's
/// view-only supervision connections, committed atomically by <c>TransactionBehavior</c>.
/// </summary>
internal sealed class SaveStudyClinicCanvasHandler(
    IStudyClinicRepository _studyClinicRepository,
    ISupervisionConnectionSyncApplicationService _supervisionConnectionSyncApplicationService)
    : IRequestHandler<SaveStudyClinicCanvasCommand, ErrorOr<Updated>>
{
    /// <summary>
    /// Applies the requested canvas state to the clinic.
    /// </summary>
    /// <param name="command">Canvas save command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated, not found, or validation errors.</returns>
    public async Task<ErrorOr<Updated>> Handle(SaveStudyClinicCanvasCommand command, CancellationToken cancellationToken)
    {
        StudyClinic? clinic = await _studyClinicRepository.GetByIdAsync(command.ClinicId, track: true, cancellationToken);
        if (clinic is null)
        {
            return StudyModeErrors.ClinicNotFound(command.ClinicId);
        }

        ErrorOr<Success> associationsResult = clinic.ValidateAssociations(command.Request.MapToAssociationPairs());
        if (associationsResult.IsError)
        {
            return associationsResult.Errors;
        }

        ErrorOr<Success> positionsResult = clinic.UpdatePositions(command.Request.MapToPositions());
        if (positionsResult.IsError)
        {
            return positionsResult.Errors;
        }

        ErrorOr<Success> connectionsResult = await _supervisionConnectionSyncApplicationService.ReplaceConnectionsAsync(
            clinic.GetMemberUserIds(StudyMemberKind.Doctor),
            clinic.GetMemberUserIds(StudyMemberKind.Patient),
            command.Request.MapToSupervisionPairs(),
            SupervisionAccessLevel.ViewOnly,
            cancellationToken);
        if (connectionsResult.IsError)
        {
            return connectionsResult.Errors;
        }

        return Result.Updated;
    }
}
