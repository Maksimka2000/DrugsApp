using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.RejectSupervisionRequest;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.RejectSupervisionRequest;

[ApiController]
[Route("api/supervision/patient/requests")]
[Tags("Supervision Patient Request")]
public sealed class RejectSupervisionRequestController(
    IMediator mediator,
    IValidator<RejectSupervisionRequestRequest> validator) : ControllerBase
{
    public const string RouteName = "RejectSupervisionRequest";

    /// <summary>
    /// Rejects a supervision request for the current patient.
    /// </summary>
    /// <param name="request_id">Identifier of the supervision request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpPost("{request_id:guid}/reject", Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<RejectSupervisionRequestResponseDto>> Reject(
        [FromRoute] Guid request_id,
        CancellationToken cancellationToken)
    {
        var request = new RejectSupervisionRequestRequest(request_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid patientUserId))
        {
            return Unauthorized();
        }

        var command = new RejectSupervisionRequestCommand(patientUserId, request.RequestId);
        ErrorOr<RejectSupervisionRequestResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<RejectSupervisionRequestResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
