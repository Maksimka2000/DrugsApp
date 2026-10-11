using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyDoctor;

/// <summary>
/// Body of the study doctor update request (same fields as creation, without position).
/// </summary>
/// <param name="FirstName">Profile first name.</param>
/// <param name="LastName">Profile last name.</param>
/// <param name="DateOfBirth">Profile date of birth.</param>
/// <param name="Gender">Profile gender.</param>
/// <param name="ProfessionCode">Provider profession.</param>
/// <param name="OtherProfessionLabel">Custom profession; required when the profession is other.</param>
/// <param name="WorkplaceLabel">Optional workplace label.</param>
/// <param name="ShortDescription">Optional short description.</param>
public sealed record UpdateStudyDoctorRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription) : IStudyPersonalDataRequest, IStudyDoctorProfileRequest;

/// <summary>
/// Command updating a study doctor's profile and provider data.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="DoctorUserId">Doctor user identifier.</param>
/// <param name="Request">Composed body.</param>
public sealed record UpdateStudyDoctorCommand(
    Guid ClinicId,
    Guid DoctorUserId,
    UpdateStudyDoctorRequest Request) : IRequest<ErrorOr<StudyDoctorNodeDto>>;

/// <summary>
/// Updates study doctors for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class UpdateStudyDoctorController(
    IMediator _mediator,
    IValidator<UpdateStudyDoctorRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateStudyDoctor";

    /// <summary>
    /// Replaces the doctor's personal data and provider profile.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="doctorUserId">Doctor user identifier.</param>
    /// <param name="request">New profile and provider data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated doctor node.</response>
    /// <response code="400">Invalid payload.</response>
    /// <response code="404">The doctor is not a member of the clinic.</response>
    [HttpPut("{clinic_id:guid}/doctors/{doctor_id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(StudyDoctorNodeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyDoctorNodeDto>> Update(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromRoute(Name = "doctor_id")] Guid doctorUserId,
        [FromBody] UpdateStudyDoctorRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateStudyDoctorCommand command = request.MapToCommand(clinicId, doctorUserId);
        ErrorOr<StudyDoctorNodeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
