using DrugsApp.Domain.StudyModeFeature.Entities;
using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Domain.StudyModeFeature.Repositories;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyClinicVisibility;

/// <summary>
/// Handles the clinic visibility change; the aggregate enforces the creator-only rule.
/// </summary>
internal sealed class UpdateStudyClinicVisibilityHandler(IStudyClinicRepository _studyClinicRepository)
    : IRequestHandler<UpdateStudyClinicVisibilityCommand, ErrorOr<StudyClinicDto>>
{
    /// <summary>
    /// Loads the tracked clinic and changes its visibility.
    /// </summary>
    /// <param name="command">Visibility change command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated clinic, not found, or forbidden for non-creators.</returns>
    public async Task<ErrorOr<StudyClinicDto>> Handle(
        UpdateStudyClinicVisibilityCommand command,
        CancellationToken cancellationToken)
    {
        StudyClinic? clinic = await _studyClinicRepository.GetByIdAsync(command.ClinicId, track: true, cancellationToken);
        if (clinic is null)
        {
            return StudyModeErrors.ClinicNotFound(command.ClinicId);
        }

        StudyClinicVisibility visibility = StudyModeMappings.MapToVisibility(command.Request.Visibility)!.Value;
        ErrorOr<Success> changeResult = clinic.ChangeVisibility(visibility, command.CallerUserId);
        if (changeResult.IsError)
        {
            return changeResult.Errors;
        }

        return clinic.MapToDto(command.CallerUserId);
    }
}
