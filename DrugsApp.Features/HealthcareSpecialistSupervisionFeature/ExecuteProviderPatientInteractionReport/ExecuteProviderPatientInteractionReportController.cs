using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionReport;

[ApiController]
[Route("api/supervision/provider/patients")]
[Tags("Supervision HealthcareSpecialist - Patient")]
public sealed class ExecuteProviderPatientInteractionReportController(IMediator mediator) : ControllerBase
{
    public const string RouteName = "ExecuteProviderPatientInteractionReport";

    /// <summary>
    /// Computes the safety report for a supervised patient's stored data.
    /// </summary>
    /// <param name="patientId">Patient user identifier.</param>
    /// <param name="request">Which report sections to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Report computed.</response>
    /// <response code="403">Caller is not a provider with a profile, or the access level forbids reading.</response>
    /// <response code="404">No active supervision connection with this patient.</response>
    [HttpPost("{patient_id:guid}/interaction-report", Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    [ProducesResponseType(typeof(ProviderPatientInteractionReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderPatientInteractionReportResponse>> Execute(
        [FromRoute(Name = "patient_id")] Guid patientId,
        [FromBody] ProviderPatientInteractionReportRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid providerUserId))
        {
            return Unauthorized();
        }

        var command = new ExecuteProviderPatientInteractionReportCommand(
            providerUserId,
            patientId,
            request);

        ErrorOr<ProviderPatientInteractionReportResponse> result =
            await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ProviderPatientInteractionReportResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
