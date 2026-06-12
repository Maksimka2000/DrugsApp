using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients;
using DrugsApp.Features.HealthcareSpecialistSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.HealthcareSpecialistSupervisionFeature.ListProviderPatients;

[ApiController]
[Route("api/supervision/provider/patients")]
[Tags("Supervision Healthcare Specialist")]
public sealed class ListProviderPatientsController(
    IMediator mediator,
    IValidator<ListProviderPatientsRequest> validator) : ControllerBase
{
    public const string RouteName = "ListProviderPatients";

    [HttpGet(Name = RouteName)]
    [Authorize(Roles = AuthRoles.HealthcareProvider)]
    public async Task<ActionResult<ProviderPatientListResponse>> List(
        [FromQuery] ListProviderPatientsRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var providerUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        ListProviderPatientsQuery query = request.MapToQuery(providerUserId);
        ErrorOr<ProviderPatientListResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ProviderPatientListResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
