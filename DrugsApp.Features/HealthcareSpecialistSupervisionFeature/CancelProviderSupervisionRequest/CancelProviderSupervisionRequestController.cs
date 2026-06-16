using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderSupervisionRequest;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderSupervisionRequest;

[ApiController]
[Route("api/supervision/provider/requests")]
[Tags("Supervision HealthcareSpecialist Request")]
public sealed class CancelProviderSupervisionRequestController(
    IMediator mediator,
    IValidator<CancelProviderSupervisionRequestRequest> validator) : ControllerBase
{
    public const string RouteName = "CancelProviderSupervisionRequest";

    [HttpPost("{request_id:guid}/cancel", Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<CancelProviderSupervisionRequestResponseDto>> Cancel(
        [FromRoute] Guid request_id,
        CancellationToken cancellationToken)
    {
        var request = new CancelProviderSupervisionRequestRequest(request_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid providerUserId))
        {
            return Unauthorized();
        }

        var command = new CancelProviderSupervisionRequestCommand(providerUserId, request.RequestId);
        ErrorOr<CancelProviderSupervisionRequestResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CancelProviderSupervisionRequestResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
