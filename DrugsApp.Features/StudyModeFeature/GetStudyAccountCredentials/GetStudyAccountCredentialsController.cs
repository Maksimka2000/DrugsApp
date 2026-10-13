using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.GetStudyAccountCredentials;

/// <summary>
/// Query revealing the plain credentials of a study account.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="UserId">Member user identifier.</param>
public sealed record GetStudyAccountCredentialsQuery(
    Guid ClinicId,
    Guid UserId) : IRequest<ErrorOr<StudyAccountCredentialsDto>>;

/// <summary>
/// Returns study account credentials for teachers and admins; responses are never cached.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class GetStudyAccountCredentialsController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "GetStudyAccountCredentials";

    /// <summary>
    /// Decrypts and returns the generated login and password of a member.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="userId">Member user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the credentials.</response>
    /// <response code="404">Clinic member was not found.</response>
    /// <response code="500">Stored credentials can no longer be decrypted.</response>
    [HttpGet("{clinic_id:guid}/members/{user_id:guid}/credentials", Name = RouteName)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(StudyAccountCredentialsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StudyAccountCredentialsDto>> Get(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromRoute(Name = "user_id")] Guid userId,
        CancellationToken cancellationToken)
    {
        GetStudyAccountCredentialsQuery query = new(clinicId, userId);
        ErrorOr<StudyAccountCredentialsDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
