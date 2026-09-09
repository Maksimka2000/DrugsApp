using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionPrecheck;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ExecuteProviderPatientInteractionPrecheck;

[ApiController]
[Route("api/supervision/provider/patients")]
[Tags("Supervision HealthcareSpecialist - Patient")]
public sealed class ExecuteProviderPatientInteractionPrecheckController(
    IMediator mediator,
    IValidator<ProviderPatientInteractionPrecheckRequest> validator) : ControllerBase
{
    public const string RouteName = "ExecuteProviderPatientInteractionPrecheck";

    [HttpPost("{patient_id:guid}/interaction-precheck", Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    [ProducesResponseType(typeof(ProviderPatientInteractionPrecheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProviderPatientInteractionPrecheckResponse>> Execute(
        [FromRoute(Name = "patient_id")] Guid patientId,
        [FromBody] ProviderPatientInteractionPrecheckRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid providerUserId))
        {
            return Unauthorized();
        }

        var command = new ExecuteProviderPatientInteractionPrecheckCommand(
            providerUserId,
            patientId,
            request);

        ErrorOr<ProviderPatientInteractionPrecheckResponse> result =
            await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ProviderPatientInteractionPrecheckResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
