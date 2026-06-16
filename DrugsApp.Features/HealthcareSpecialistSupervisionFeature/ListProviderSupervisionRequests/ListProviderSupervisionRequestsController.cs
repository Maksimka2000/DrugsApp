using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderSupervisionRequests;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderSupervisionRequests;

[ApiController]
[Route("api/supervision/provider/requests")]
[Tags("Supervision HealthcareSpecialist Request")]
public sealed class ListProviderSupervisionRequestsController(
    IMediator mediator,
    IValidator<ListProviderSupervisionRequestsRequest> validator) : ControllerBase
{
    public const string RouteName = "ListProviderSupervisionRequests";

    [HttpGet(Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<ProviderSupervisionRequestListResponse>> List(
        [FromQuery] ListProviderSupervisionRequestsRequest request,
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

        ListProviderSupervisionRequestsQuery query = request.MapToQuery(providerUserId);
        ErrorOr<ProviderSupervisionRequestListResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ProviderSupervisionRequestListResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
