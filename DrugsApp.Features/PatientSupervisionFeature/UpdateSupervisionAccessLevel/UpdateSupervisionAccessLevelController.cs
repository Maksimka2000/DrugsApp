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
[Route("api/supervision/patient/healthcare-professionals")]
[Tags("Supervision Patient - Healthcare Professional")]
public sealed class UpdateSupervisionAccessLevelController(
    IMediator mediator,
    IValidator<UpdateSupervisionAccessLevelRequest> validator) : ControllerBase
{
    public const string RouteName = "UpdateSupervisionAccessLevel";

    [HttpPut("{healthcare_professional_user_id:guid}/access-level", Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<UpdateSupervisionAccessLevelResponseDto>> Update(
        [FromRoute(Name = "healthcare_professional_user_id")] Guid healthcareProfessionalUserId,
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
            HealthcareProfessionalUserId: healthcareProfessionalUserId,
            AccessLevelCode: request.AccessLevelCode);

        ErrorOr<UpdateSupervisionAccessLevelResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateSupervisionAccessLevelResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
