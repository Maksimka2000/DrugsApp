using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.PatientSupervisionFeature.GetPatientHealthcareProfessional;
using DrugsApp.Features.PatientSupervisionFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace DrugsApp.Features.PatientSupervisionFeature.GetPatientHealthcareProfessional;

[ApiController]
[Route("api/supervision/patient/healthcare-professionals")]
[Tags("Supervision Patient - Healthcare Professional")]
public sealed class GetPatientHealthcareProfessionalController(
    IMediator mediator,
    IValidator<GetPatientHealthcareProfessionalRequest> validator) : ControllerBase
{
    public const string RouteName = "GetPatientHealthcareProfessional";

    [HttpGet("{healthcare_professional_user_id:guid}", Name = RouteName)]
    [Authorize(Roles = AuthRoles.MemberRoles)]
    public async Task<ActionResult<PatientHealthcareProfessionalDetailDto>> GetById(
        [FromRoute] Guid healthcare_professional_user_id,
        CancellationToken cancellationToken)
    {
        var request = new GetPatientHealthcareProfessionalRequest(healthcare_professional_user_id);

        ValidationResult? validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid patientUserId))
        {
            return Unauthorized();
        }

        var query = new GetPatientHealthcareProfessionalQuery(patientUserId, healthcare_professional_user_id);
        ErrorOr<PatientHealthcareProfessionalDetailDto> result = await mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<PatientHealthcareProfessionalDetailDto>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
