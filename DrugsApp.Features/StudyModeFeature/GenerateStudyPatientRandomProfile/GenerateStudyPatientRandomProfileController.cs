using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.GenerateStudyPatientRandomProfile;

/// <summary>
/// Query producing a random personal data suggestion for a new study patient.
/// </summary>
public sealed record GenerateStudyPatientRandomProfileQuery : IRequest<ErrorOr<StudyRandomPatientProfileResponse>>;

/// <summary>
/// Suggests a random Polish patient profile for the Study Patient modal for teachers and admins.
/// </summary>
[ApiController]
[Tags("Study Mode")]
[Route("api/admin/study/random-profiles")]
public sealed class GenerateStudyPatientRandomProfileController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "GenerateStudyPatientRandomProfile";

    /// <summary>
    /// Returns a random first name, surname, birth date (18–90 years old) and gender; nothing is saved, the teacher
    /// can edit the values before creating the patient.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the suggested profile.</response>
    [HttpGet("patient", Name = RouteName)]
    [ProducesResponseType(typeof(StudyRandomPatientProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudyRandomPatientProfileResponse>> Generate(CancellationToken cancellationToken)
    {
        GenerateStudyPatientRandomProfileQuery query = new();
        ErrorOr<StudyRandomPatientProfileResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
