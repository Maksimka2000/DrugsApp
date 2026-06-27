using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;

[ApiController]
[Route("api/supervision/patient/healthcare-professionals")]
[Tags("Supervision Patient - Healthcare Professional")]
public sealed class RemoveSupervisionConnectionController(IMediator mediator) : ControllerBase
{
    public const string RouteName = "RemoveSupervisionConnection";

    /// <summary>
    /// Removes the supervision connection between the current patient and a healthcare professional.
    /// </summary>
    /// <param name="healthcareProfessionalUserId">Healthcare professional identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpDelete("{healthcare_professional_user_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<RemoveSupervisionConnectionResponseDto>> Remove(
        [FromRoute(Name = "healthcare_professional_user_id")] Guid healthcareProfessionalUserId,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out Guid callerUserId))
        {
            return Unauthorized();
        }

        var command = new RemoveSupervisionConnectionCommand(
            CallerUserId: callerUserId,
            ExpectedHealthcareProfessionalUserId: healthcareProfessionalUserId);

        ErrorOr<RemoveSupervisionConnectionResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<RemoveSupervisionConnectionResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
