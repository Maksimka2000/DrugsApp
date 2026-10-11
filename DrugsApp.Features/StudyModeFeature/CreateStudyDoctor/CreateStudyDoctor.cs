using DrugsApp.Features.StudyModeFeature.ApplicationServices;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyDoctor;

/// <summary>
/// Handles study doctor creation by delegating to the shared provisioning service; the Identity user, profiles
/// and member row are committed together by <c>TransactionBehavior</c>.
/// </summary>
internal sealed class CreateStudyDoctorHandler(
    IStudyAccountProvisioningApplicationService _studyAccountProvisioningApplicationService)
    : IRequestHandler<CreateStudyDoctorCommand, ErrorOr<CreatedStudyAccountResponse>>
{
    /// <summary>
    /// Provisions the doctor account and maps the node with its one-time credentials.
    /// </summary>
    /// <param name="command">Creation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created node with credentials, or errors.</returns>
    public async Task<ErrorOr<CreatedStudyAccountResponse>> Handle(
        CreateStudyDoctorCommand command,
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
