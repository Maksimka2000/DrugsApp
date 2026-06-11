using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.ListPatientHealthcareProfessionals;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.ListPatientHealthcareProfessionals;

[ApiController]
[Route("api/supervision/patient/healthcare-professionals")]
[Tags("Supervision")]
public sealed class ListPatientHealthcareProfessionalsController(
    IMediator mediator,
    IValidator<SearchPatientHealthcareProfessionalsRequest> validator) : ControllerBase
{
    public const string RouteName = "ListPatientHealthcareProfessionals";

    [HttpGet(Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<PatientHealthcareProfessionalListResponse>> List(
        [FromQuery] SearchPatientHealthcareProfessionalsRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        var patientUserId = Guid.Parse(User.FindFirst("sub")!.Value);

        SearchPatientHealthcareProfessionalsQuery query = request.MapToQuery(patientUserId);
        ErrorOr<PatientHealthcareProfessionalListResponse> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<PatientHealthcareProfessionalListResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
