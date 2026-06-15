using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.UpdateSupervisionAccessLevel;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.UpdateSupervisionAccessLevel;

[ApiController]
[Route("api/supervision/connections")]
[Tags("Supervision Patient Connection")]
public sealed class UpdateSupervisionAccessLevelController(
    IMediator mediator,
    IValidator<UpdateSupervisionAccessLevelRequest> validator) : ControllerBase
{
    public const string RouteName = "UpdateSupervisionAccessLevel";

    [HttpPut("{connection_id:guid}/access-level", Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<UpdateSupervisionAccessLevelResponseDto>> Update(
        [FromRoute] Guid connection_id,
        [FromBody] UpdateSupervisionAccessLevelRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid patientUserId))
        {
            return Unauthorized();
        }

        var command = new UpdateSupervisionAccessLevelCommand(
            PatientUserId: patientUserId,
            ConnectionId: connection_id,
            AccessLevelCode: request.AccessLevelCode);

        ErrorOr<UpdateSupervisionAccessLevelResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateSupervisionAccessLevelResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
