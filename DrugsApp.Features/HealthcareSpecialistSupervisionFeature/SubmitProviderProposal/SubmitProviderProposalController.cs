using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.SubmitProviderProposal;

[ApiController]
[Route("api/supervision/provider/patients/{patient_id:guid}/proposals")]
[Tags("Supervision Healthcare Specialist")]
public sealed class SubmitProviderProposalController(
    IMediator mediator,
    IValidator<SubmitProviderProposalRequest> validator) : ControllerBase
{
    public const string RouteName = "SubmitProviderProposal";

    [HttpPost(Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<SubmitProviderProposalResponseDto>> Submit(
        [FromRoute(Name = "patient_id")] Guid patientId,
        [FromBody] SubmitProviderProposalRequest request,
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

        SubmitProviderProposalCommand command = request.MapToCommand(providerUserId, patientId);

        ErrorOr<SubmitProviderProposalResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<SubmitProviderProposalResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
