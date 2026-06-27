using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.RejectPatientProposal;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.RejectPatientProposal;

[ApiController]
[Route("api/supervision/patient/proposals")]
[Tags("Supervision Patient Proposal")]
public sealed class RejectPatientProposalController(
    IMediator mediator,
    IValidator<RejectPatientProposalRequest> validator) : ControllerBase
{
    public const string RouteName = "RejectPatientProposal";

    /// <summary>
    /// Rejects a patient proposal.
    /// </summary>
    /// <param name="proposal_id">Proposal identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    [HttpPost("{proposal_id:guid}/reject", Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<RejectPatientProposalResponseDto>> Reject(
        [FromRoute] Guid proposal_id,
        CancellationToken cancellationToken)
    {
        var request = new RejectPatientProposalRequest(proposal_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid patientUserId))
        {
            return Unauthorized();
        }

        var command = new RejectPatientProposalCommand(patientUserId, proposal_id);
        ErrorOr<RejectPatientProposalResponseDto> result = await mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<RejectPatientProposalResponseDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
