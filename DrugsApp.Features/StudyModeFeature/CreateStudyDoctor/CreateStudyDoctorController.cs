using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.GetStudyDoctor;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyDoctor;

/// <summary>
/// Body of the study doctor creation request.
/// </summary>
/// <param name="FirstName">Profile first name (1–100 characters).</param>
/// <param name="LastName">Profile last name (1–100 characters).</param>
/// <param name="DateOfBirth">Profile date of birth (not in the future).</param>
/// <param name="Gender">Profile gender (female, male, prefer_not_to_say).</param>
/// <param name="ProfessionCode">Provider profession (doctor, nurse, pharmacist, dietitian, therapist, other).</param>
/// <param name="OtherProfessionLabel">Custom profession; required when the profession is other.</param>
/// <param name="WorkplaceLabel">Optional workplace label.</param>
/// <param name="ShortDescription">Optional short description.</param>
/// <param name="PositionX">Initial horizontal canvas coordinate.</param>
/// <param name="PositionY">Initial vertical canvas coordinate.</param>
public sealed record CreateStudyDoctorRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    string ProfessionCode,
    string? OtherProfessionLabel,
    string? WorkplaceLabel,
    string? ShortDescription,
    double PositionX,
    double PositionY) : IStudyPersonalDataRequest, IStudyDoctorProfileRequest;

/// <summary>
/// Command provisioning a study doctor account in a clinic.
/// </summary>
/// <param name="ClinicId">Target clinic identifier.</param>
/// <param name="Request">Composed body.</param>
public sealed record CreateStudyDoctorCommand(
    Guid ClinicId,
    CreateStudyDoctorRequest Request) : IRequest<ErrorOr<CreatedStudyAccountResponse>>;

/// <summary>
/// Provisions study doctor accounts for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class CreateStudyDoctorController(
    IMediator _mediator,
    IValidator<CreateStudyDoctorRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "CreateStudyDoctor";

    /// <summary>
    /// Creates a read-only study doctor account and returns its node plus one-time plain credentials.
    /// </summary>
    /// <param name="clinicId">Target clinic identifier.</param>
    /// <param name="request">Personal, provider and position data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Doctor created; credentials are returned once.</response>
    /// <response code="400">Invalid payload.</response>
    /// <response code="404">Clinic was not found.</response>
    /// <response code="409">The clinic already holds the maximum number of doctors.</response>
    [HttpPost("{clinic_id:guid}/doctors", Name = RouteName)]
    [ProducesResponseType(typeof(CreatedStudyAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedStudyAccountResponse>> Create(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromBody] CreateStudyDoctorRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateStudyDoctorCommand command = request.MapToCommand(clinicId);
        ErrorOr<CreatedStudyAccountResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: GetStudyDoctorController.RouteName,
                routeValues: new { clinic_id = clinicId, doctor_id = response.Doctor!.UserId },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
