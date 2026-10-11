using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyPatient;

/// <summary>
/// Handles study patient creation by delegating to the shared provisioning service (no provider profile, no
/// reproductive health profile).
/// </summary>
internal sealed class CreateStudyPatientHandler(
    IStudyAccountProvisioningApplicationService _studyAccountProvisioningApplicationService)
    : IRequestHandler<CreateStudyPatientCommand, ErrorOr<CreatedStudyAccountResponse>>
{
    /// <summary>
    /// Provisions the patient account and maps the node with an all-zero medical summary and the credentials.
    /// </summary>
    /// <param name="command">Creation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created node with credentials, or errors.</returns>
    public async Task<ErrorOr<CreatedStudyAccountResponse>> Handle(
        CreateStudyPatientCommand command,
        CancellationToken cancellationToken)
    {
        StudyAccountProvisioningDraft draft = command.Request.MapToDraft();

        ErrorOr<StudyAccountProvisioningResult> provisioningResult =
            await _studyAccountProvisioningApplicationService.ProvisionAsync(command.ClinicId, draft, cancellationToken);
        if (provisioningResult.IsError)
        {
            return provisioningResult.Errors;
        }

        return provisioningResult.Value.MapToResponse(draft);
    }
}
