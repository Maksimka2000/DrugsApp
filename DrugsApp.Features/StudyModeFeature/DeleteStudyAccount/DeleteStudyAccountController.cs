using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.DeleteStudyAccount;

/// <summary>
/// Command deleting one study account (doctor or patient) of a clinic.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="UserId">Member user identifier.</param>
public sealed record DeleteStudyAccountCommand(
    Guid ClinicId,
    Guid UserId) : IRequest<ErrorOr<Deleted>>;

/// <summary>
/// Deletes study accounts for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class DeleteStudyAccountController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "DeleteStudyAccount";

    /// <summary>
    /// Deletes the study account with all its owned data; its supervision edges disappear with it.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="userId">Member user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Study account deleted.</response>
    /// <response code="404">Clinic or member was not found.</response>
    [HttpDelete("{clinic_id:guid}/members/{user_id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromRoute(Name = "user_id")] Guid userId,
        CancellationToken cancellationToken)
    {
        DeleteStudyAccountCommand command = new(clinicId, userId);
        ErrorOr<Deleted> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
