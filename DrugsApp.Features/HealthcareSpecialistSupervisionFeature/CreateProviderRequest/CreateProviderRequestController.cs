using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CreateProviderRequest;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.CreateProviderRequest;

[ApiController]
[Route("api/supervision/provider/requests")]
[Tags("Supervision Healthcare Specialist")]
public sealed class CreateProviderRequestController(
    IMediator mediator,
    IValidator<CreateProviderRequestRequest> validator) : ControllerBase
{
    public const string RouteName = "CreateProviderRequest";

    [HttpPost(Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<CreateProviderRequestResponseDto>> Create(
        [FromBody] CreateProviderRequestRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid providerUserId))
        {
            return Unauthorized();
        }

        CreateProviderRequestCommand command = request.MapToCommand(providerUserId);
        ErrorOr<CreateProviderRequestResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreateProviderRequestResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
