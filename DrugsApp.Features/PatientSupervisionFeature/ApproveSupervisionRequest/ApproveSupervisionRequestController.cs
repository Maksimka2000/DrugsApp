using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.ApproveSupervisionRequest;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.ApproveSupervisionRequest;

[ApiController]
[Route("api/supervision/requests")]
[Tags("Supervision request")]
public sealed class ApproveSupervisionRequestController(
    IMediator mediator,
    IValidator<ApproveSupervisionRequestRequest> validator) : ControllerBase
{
    public const string RouteName = "ApproveSupervisionRequest";

    [HttpPost("{request_id:guid}/approve", Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<ApproveSupervisionRequestResponseDto>> Approve(
        [FromRoute] Guid request_id,
        [FromBody] ApproveSupervisionRequestRequest body,
        CancellationToken cancellationToken)
    {
        // merge route id into request
        ApproveSupervisionRequestRequest request = body with { RequestId = request_id };

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid patientUserId))
        {
            return Unauthorized();
        }

        var command = new ApproveSupervisionRequestCommand(
            PatientUserId: patientUserId,
            RequestId: request.RequestId,
            AccessLevelCode: request.AccessLevelCode);

        ErrorOr<ApproveSupervisionRequestResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ApproveSupervisionRequestResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
