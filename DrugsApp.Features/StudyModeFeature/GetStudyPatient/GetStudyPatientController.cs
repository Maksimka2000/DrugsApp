using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.GetStudyPatient;

/// <summary>
/// Query loading a study patient's profile tab data.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="PatientUserId">Patient user identifier.</param>
public sealed record GetStudyPatientQuery(
    Guid ClinicId,
    Guid PatientUserId) : IRequest<ErrorOr<StudyPatientDetailDto>>;

/// <summary>
/// Returns study patient details for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class GetStudyPatientController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "GetStudyPatient";

    /// <summary>
    /// Loads profile data and medical record counters of a patient member of the clinic.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the patient details.</response>
    /// <response code="404">The patient is not a member of the clinic.</response>
    [HttpGet("{clinic_id:guid}/patients/{patient_id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(StudyPatientDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyPatientDetailDto>> Get(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken)
    {
        GetStudyPatientQuery query = new(clinicId, patientUserId);
        ErrorOr<StudyPatientDetailDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
