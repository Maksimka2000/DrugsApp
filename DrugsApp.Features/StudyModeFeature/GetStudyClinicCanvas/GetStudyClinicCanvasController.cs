using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.GetStudyClinicCanvas;

/// <summary>
/// Query loading the complete canvas of a study clinic.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="CallerUserId">Calling admin user, used for the ownership flag.</param>
public sealed record GetStudyClinicCanvasQuery(Guid ClinicId, Guid CallerUserId) : IRequest<ErrorOr<StudyClinicCanvasResponse>>;

/// <summary>
/// Returns the study clinic canvas (header, doctor and patient nodes, associations).
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class GetStudyClinicCanvasController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "GetStudyClinicCanvas";

    /// <summary>
    /// Loads the clinic header, every doctor and patient node and the active associations; passwords are never
    /// included.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the canvas state.</response>
    /// <response code="404">Clinic was not found.</response>
    [HttpGet("{clinic_id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(StudyClinicCanvasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyClinicCanvasResponse>> Get(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid callerUserId))
        {
            return Unauthorized();
        }

        GetStudyClinicCanvasQuery query = new(clinicId, callerUserId);
        ErrorOr<StudyClinicCanvasResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
