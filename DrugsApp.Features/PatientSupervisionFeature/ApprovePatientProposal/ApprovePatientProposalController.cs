using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.ApprovePatientProposal;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.ApprovePatientProposal;

[ApiController]
[Route("api/supervision/patient/proposals")]
[Tags("Supervision Patient")]
public sealed class ApprovePatientProposalController(
    IMediator mediator,
    IValidator<ApprovePatientProposalRequest> validator) : ControllerBase
{
    public const string RouteName = "ApprovePatientProposal";

    [HttpPost("{proposal_id:guid}/approve", Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<ApprovePatientProposalResponseDto>> Approve(
        [FromRoute] Guid proposal_id,
        CancellationToken cancellationToken)
    {
        var request = new ApprovePatientProposalRequest(proposal_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid patientUserId))
        {
            return Unauthorized();
        }

        var command = new ApprovePatientProposalCommand(patientUserId, proposal_id);
        ErrorOr<ApprovePatientProposalResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<ApprovePatientProposalResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
