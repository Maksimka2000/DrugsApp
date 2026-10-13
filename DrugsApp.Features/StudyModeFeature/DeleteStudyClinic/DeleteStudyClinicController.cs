using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.DeleteStudyClinic;

/// <summary>
/// Command deleting a study clinic together with every study account it holds.
/// </summary>
/// <param name="ClinicId">Clinic to delete.</param>
public sealed record DeleteStudyClinicCommand(Guid ClinicId) : IRequest<ErrorOr<Deleted>>;

/// <summary>
/// Deletes study clinics for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class DeleteStudyClinicController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "DeleteStudyClinic";

    /// <summary>
    /// Deletes the clinic after tearing down every member account and its owned data in one transaction.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Clinic and all its study accounts were deleted.</response>
    /// <response code="404">Clinic was not found.</response>
    [HttpDelete("{clinic_id:guid}", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        CancellationToken cancellationToken)
    {
        DeleteStudyClinicCommand command = new(clinicId);
        ErrorOr<Deleted> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
