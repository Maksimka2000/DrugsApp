using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.ListPatientProposals;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientProposals;

[ApiController]
[Route("api/supervision/patient/proposals")]
[Tags("Supervision Patient")]
public sealed class ListPatientProposalsController(
    IMediator mediator,
    IValidator<ListPatientProposalsRequest> validator) : ControllerBase
{
    public const string RouteName = "ListPatientProposals";

    [HttpGet(Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<PatientProposalListResponse>> List(
        [FromQuery] ListPatientProposalsRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var patientUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        ListPatientProposalsQuery query = request.MapToQuery(patientUserId);
        ErrorOr<PatientProposalListResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<PatientProposalListResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
