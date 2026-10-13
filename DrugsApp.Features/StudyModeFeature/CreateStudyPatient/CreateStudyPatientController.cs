using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.GetStudyPatient;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyPatient;

/// <summary>
/// Body of the study patient creation request.
/// </summary>
/// <param name="FirstName">Profile first name (1–100 characters).</param>
/// <param name="LastName">Profile last name (1–100 characters).</param>
/// <param name="DateOfBirth">Profile date of birth (not in the future).</param>
/// <param name="Gender">Profile gender (female, male, prefer_not_to_say).</param>
/// <param name="PositionX">Initial horizontal canvas coordinate.</param>
/// <param name="PositionY">Initial vertical canvas coordinate.</param>
public sealed record CreateStudyPatientRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Gender,
    double PositionX,
    double PositionY) : IStudyPersonalDataRequest;

/// <summary>
/// Command provisioning a study patient account in a clinic.
/// </summary>
/// <param name="ClinicId">Target clinic identifier.</param>
/// <param name="Request">Composed body.</param>
public sealed record CreateStudyPatientCommand(
    Guid ClinicId,
    CreateStudyPatientRequest Request) : IRequest<ErrorOr<CreatedStudyAccountResponse>>;

/// <summary>
/// Provisions study patient accounts for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class CreateStudyPatientController(
    IMediator _mediator,
    IValidator<CreateStudyPatientRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "CreateStudyPatient";

    /// <summary>
    /// Creates a read-only study patient account and returns its node plus one-time plain credentials.
    /// </summary>
    /// <param name="clinicId">Target clinic identifier.</param>
    /// <param name="request">Personal and position data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Patient created; credentials are returned once.</response>
    /// <response code="400">Invalid payload.</response>
    /// <response code="404">Clinic was not found.</response>
    /// <response code="409">The clinic already holds the maximum number of patients.</response>
    [HttpPost("{clinic_id:guid}/patients", Name = RouteName)]
    [ProducesResponseType(typeof(CreatedStudyAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedStudyAccountResponse>> Create(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromBody] CreateStudyPatientRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateStudyPatientCommand command = request.MapToCommand(clinicId);
        ErrorOr<CreatedStudyAccountResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: GetStudyPatientController.RouteName,
                routeValues: new { clinic_id = clinicId, patient_id = response.Patient!.UserId },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
