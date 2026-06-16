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
[Route("api/supervision/patient/requests")]
[Tags("Supervision Patient Request")]
public sealed class ApproveSupervisionRequestController(
    IMediator mediator,
    IValidator<ApproveSupervisionRequestRequest> validator) : ControllerBase
{
    public const string RouteName = "ApproveSupervisionRequest";

    [HttpPost("{request_id:guid}/approve", Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<ApproveSupervisionRequestResponseDto>> Approve(
        [FromRoute] Guid request_id,
        [FromBody] ApproveSupervisionRequestRequest request,
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

        var command = new ApproveSupervisionRequestCommand(
            PatientUserId: patientUserId,
            RequestId: request_id,
            AccessLevelCode: request.AccessLevelCode);

        ErrorOr<ApproveSupervisionRequestResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ApproveSupervisionRequestResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
