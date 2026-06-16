using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.RemoveProviderPatientConnection;

[ApiController]
[Route("api/supervision/provider/patients")]
[Tags("Supervision HealthcareSpecialist - Patient")]
public sealed class RemoveProviderPatientConnectionController(IMediator mediator) : ControllerBase
{
    public const string RouteName = "RemoveProviderPatientConnection";

    [HttpDelete("{patient_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<RemoveSupervisionConnectionResponseDto>> Remove(
        [FromRoute(Name = "patient_id")] Guid patientId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid providerUserId))
        {
            return Unauthorized();
        }

        var command = new RemoveSupervisionConnectionCommand(
            CallerUserId: providerUserId,
            ExpectedPatientUserId: patientId);

        ErrorOr<RemoveSupervisionConnectionResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<RemoveSupervisionConnectionResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
