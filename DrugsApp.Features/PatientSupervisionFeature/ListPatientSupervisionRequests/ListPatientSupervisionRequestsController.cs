using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.ListPatientSupervisionRequests;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientSupervisionRequests;

[ApiController]
[Route("api/supervision/patient/requests")]
[Tags("Supervision Patient Request")]
public sealed class ListPatientSupervisionRequestsController(
    IMediator mediator,
    IValidator<ListPatientSupervisionRequestsRequest> validator) : ControllerBase
{
    public const string RouteName = "ListPatientSupervisionRequests";

    [HttpGet(Name = RouteName)]
    [Authorize(Roles = AuthRoles.Client)]
    public async Task<ActionResult<PatientSupervisionRequestListResponse>> List(
        [FromQuery] ListPatientSupervisionRequestsRequest request,
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

        ListPatientSupervisionRequestsQuery query = request.MapToQuery(patientUserId);
        ErrorOr<PatientSupervisionRequestListResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<PatientSupervisionRequestListResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
