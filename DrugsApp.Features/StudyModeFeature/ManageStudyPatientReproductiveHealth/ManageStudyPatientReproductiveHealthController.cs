using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using DrugsApp.Features.UserProfileFeature.AddReproductiveHealthProfile;
using DrugsApp.Features.UserProfileFeature.GetReproductiveHealthProfile;
using DrugsApp.Features.UserProfileFeature.RemoveReproductiveHealthProfile;
using DrugsApp.Features.UserProfileFeature.Shared;
using DrugsApp.Features.UserProfileFeature.UpdateReproductiveHealthProfile;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientReproductiveHealth;

/// <summary>
/// Proxies the client reproductive health profile endpoints so a teacher can author the record of a study patient;
/// every action re-dispatches the unchanged client command or query with the route patient as the acting user.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/user-profile/reproductive-health")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientReproductiveHealthController(
    IMediator _mediator,
    IValidator<AddReproductiveHealthProfileRequest> _addValidator,
    IValidator<UpdateReproductiveHealthProfileRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Returns the study patient's reproductive health profile.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reproductive health profile.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ReproductiveHealthProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReproductiveHealthProfileResponse>> Get(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken)
    {
        GetReproductiveHealthProfileQuery query = new(patientUserId);
        ErrorOr<ReproductiveHealthProfileResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds a reproductive health profile to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client add request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created profile (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ReproductiveHealthProfileResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReproductiveHealthProfileResponse>> Add(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] AddReproductiveHealthProfileRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _addValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddReproductiveHealthProfileCommand command = request.MapToCommand(patientUserId);
        ErrorOr<ReproductiveHealthProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates the study patient's reproductive health profile.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated profile.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(ReproductiveHealthProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReproductiveHealthProfileResponse>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] UpdateReproductiveHealthProfileRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateReproductiveHealthProfileCommand command = request.MapToCommand(patientUserId);
        ErrorOr<ReproductiveHealthProfileResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Removes the study patient's reproductive health profile.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken)
    {
        RemoveReproductiveHealthProfileCommand command = new(patientUserId);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
