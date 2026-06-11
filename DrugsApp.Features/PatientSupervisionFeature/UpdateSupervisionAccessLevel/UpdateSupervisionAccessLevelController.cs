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
[Tags("Supervision")]
public sealed class UpdateSupervisionAccessLevelController(
    IMediator mediator,
    IValidator<UpdateSupervisionAccessLevelRequest> validator) : ControllerBase
{
    public const string RouteName = "UpdateSupervisionAccessLevel";

    [HttpPut("{connection_id:guid}/access-level", Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<UpdateSupervisionAccessLevelResponseDto>> Update(
        [FromRoute] Guid connection_id,
        [FromBody] UpdateSupervisionAccessLevelRequest body,
        CancellationToken cancellationToken)
    {
        UpdateSupervisionAccessLevelRequest request = body with { ConnectionId = connection_id };

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var patientUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        var command = new UpdateSupervisionAccessLevelCommand(
            PatientUserId: patientUserId,
            ConnectionId: request.ConnectionId,
            AccessLevelCode: request.AccessLevelCode);

        ErrorOr<UpdateSupervisionAccessLevelResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateSupervisionAccessLevelResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
