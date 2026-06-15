using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.RemoveSupervisionConnection;

[ApiController]
[Route("api/supervision/connections")]
[Tags("Supervision Shared Connection")]
public sealed class RemoveSupervisionConnectionController(
    IMediator mediator,
    IValidator<RemoveSupervisionConnectionRequest> validator) : ControllerBase
{
    public const string RouteName = "RemoveSupervisionConnection";

    [HttpDelete("{connection_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<RemoveSupervisionConnectionResponseDto>> Remove(
        [FromRoute] Guid connection_id,
        CancellationToken cancellationToken)
    {
        var request = new RemoveSupervisionConnectionRequest(connection_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid callerUserId))
        {
            return Unauthorized();
        }

        var command = new RemoveSupervisionConnectionCommand(
            CallerUserId: callerUserId,
            ConnectionId: request.ConnectionId);

        ErrorOr<RemoveSupervisionConnectionResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<RemoveSupervisionConnectionResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
