using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.GetPatientProposal;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientProposal;

[ApiController]
[Route("api/supervision/patient/proposals")]
[Tags("Supervision")]
public sealed class GetPatientProposalController(
    IMediator mediator,
    IValidator<GetPatientProposalRequest> validator) : ControllerBase
{
    public const string RouteName = "GetPatientProposal";

    [HttpGet("{proposal_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<PatientProposalDetailResponse>> Get(
        [FromRoute] Guid proposal_id,
        CancellationToken cancellationToken)
    {
        var request = new GetPatientProposalRequest(proposal_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var patientUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        var query = new GetPatientProposalQuery(patientUserId, proposal_id);
        ErrorOr<PatientProposalDetailResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<PatientProposalDetailResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
