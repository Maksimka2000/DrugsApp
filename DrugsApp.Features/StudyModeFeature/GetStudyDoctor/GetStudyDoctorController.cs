using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.GetStudyDoctor;

/// <summary>
/// Query loading a study doctor's edit-modal data.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="DoctorUserId">Doctor user identifier.</param>
public sealed record GetStudyDoctorQuery(
    Guid ClinicId,
    Guid DoctorUserId) : IRequest<ErrorOr<StudyDoctorDetailDto>>;

/// <summary>
/// Returns study doctor details for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class GetStudyDoctorController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "GetStudyDoctor";

    /// <summary>
    /// Loads profile and provider data of a doctor member of the clinic.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="doctorUserId">Doctor user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the doctor details.</response>
    /// <response code="404">The doctor is not a member of the clinic.</response>
    [HttpGet("{clinic_id:guid}/doctors/{doctor_id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(StudyDoctorDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyDoctorDetailDto>> Get(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromRoute(Name = "doctor_id")] Guid doctorUserId,
        CancellationToken cancellationToken)
    {
        GetStudyDoctorQuery query = new(clinicId, doctorUserId);
        ErrorOr<StudyDoctorDetailDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
