using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyPatient;

/// <summary>
/// Body of the study patient update request.
/// </summary>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="DateOfBirth">Profile date of birth.</param>
/// <param name="Gender">Profile gender.</param>
public sealed record UpdateStudyPatientRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender) : IStudyPersonalDataRequest;

/// <summary>
/// Command updating a study patient's personal data.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="PatientUserId">Patient user identifier.</param>
/// <param name="Request">Composed body.</param>
public sealed record UpdateStudyPatientCommand(
    Guid ClinicId,
    Guid PatientUserId,
    UpdateStudyPatientRequest Request) : IRequest<ErrorOr<StudyPatientNodeDto>>;

/// <summary>
/// Updates study patients for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class UpdateStudyPatientController(
    IMediator _mediator,
    IValidator<UpdateStudyPatientRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateStudyPatient";

    /// <summary>
    /// Replaces the patient's personal data; changing gender away from female removes the reproductive health
    /// profile.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="patientUserId">Patient user identifier.</param>
    /// <param name="request">New personal data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated patient node.</response>
    /// <response code="400">Invalid payload.</response>
    /// <response code="404">The patient is not a member of the clinic.</response>
    [HttpPut("{clinic_id:guid}/patients/{patient_id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(StudyPatientNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyPatientNodeDto>> Update(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] UpdateStudyPatientRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateStudyPatientCommand command = request.MapToCommand(clinicId, patientUserId);
        ErrorOr<StudyPatientNodeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
