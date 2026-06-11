using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.GetSupervisionRequest;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.GetSupervisionRequest;

[ApiController]
[Route("api/supervision/requests")]
[Tags("Supervision")]
public sealed class GetSupervisionRequestController(
    IMediator mediator,
    IValidator<GetSupervisionRequestRequest> validator) : ControllerBase
{
    public const string RouteName = "GetSupervisionRequest";

    [HttpGet("{request_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<SupervisionRequestDetailDto>> Get(
        [FromRoute] Guid request_id,
        CancellationToken cancellationToken)
    {
        var request = new GetSupervisionRequestRequest(request_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var patientUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        var query = new GetSupervisionRequestQuery(patientUserId, request_id);
        ErrorOr<SupervisionRequestDetailDto> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<SupervisionRequestDetailDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
