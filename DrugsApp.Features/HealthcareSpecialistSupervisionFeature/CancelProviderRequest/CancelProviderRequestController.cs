using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderRequest;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CancelProviderRequest;

[ApiController]
[Route("api/supervision/requests")]
[Tags("Supervision Healthcare Specialist")]
public sealed class CancelProviderRequestController(
    IMediator mediator,
    IValidator<CancelProviderRequestRequest> validator) : ControllerBase
{
    public const string RouteName = "CancelProviderRequest";

    [HttpDelete("{request_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<CancelProviderRequestResponseDto>> Cancel(
        [FromRoute] Guid request_id,
        CancellationToken cancellationToken)
    {
        var request = new CancelProviderRequestRequest(request_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var providerUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        var command = new CancelProviderRequestCommand(providerUserId, request.RequestId);
        ErrorOr<CancelProviderRequestResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CancelProviderRequestResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
