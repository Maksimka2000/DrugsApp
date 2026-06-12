using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.GetProviderPatientDetail;

[ApiController]
[Route("api/supervision/provider/patients")]
[Tags("Supervision Healthcare Specialist")]
public sealed class GetProviderPatientDetailController(
    IMediator mediator,
    IValidator<GetProviderPatientDetailRequest> validator) : ControllerBase
{
    public const string RouteName = "GetProviderPatientDetail";

    [HttpGet("{connection_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<ProviderPatientDetailResponse>> Get(
        [FromRoute] Guid connection_id,
        CancellationToken cancellationToken)
    {
        var request = new GetProviderPatientDetailRequest(connection_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var providerUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        var query = new GetProviderPatientDetailQuery(providerUserId, connection_id);
        ErrorOr<ProviderPatientDetailResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ProviderPatientDetailResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
