using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.HealthConditionFeature.AddUserHealthCondition;
using DrugsApp.Features.HealthConditionFeature.CheckUserHealthConditionCustomNameAvailability;
using DrugsApp.Features.HealthConditionFeature.RemoveUserHealthCondition;
using DrugsApp.Features.HealthConditionFeature.SearchUserHealthConditions;
using DrugsApp.Features.HealthConditionFeature.Shared;
using DrugsApp.Features.HealthConditionFeature.UpdateUserHealthConditionNote;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientHealthConditions;

/// <summary>
/// Proxies the client user health condition endpoints for a study patient.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/user-health-conditions")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientHealthConditionsController(
    IMediator _mediator,
    IValidator<SearchUserHealthConditionsRequest> _searchValidator,
    IValidator<CheckUserHealthConditionCustomNameAvailabilityRequest> _checkValidator,
    IValidator<AddUserHealthConditionRequest> _addValidator,
    IValidator<UpdateUserHealthConditionNoteRequest> _updateNoteValidator) : ControllerBase
{
    /// <summary>
    /// Searches the study patient's health conditions.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="displayNameQuery">Optional display name filter.</param>
    /// <param name="skip">Paging offset.</param>
    /// <param name="take">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of health conditions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SearchUserHealthConditionsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchUserHealthConditionsResponse>> Search(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "display_name_query")] string? displayNameQuery,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 10,
        CancellationToken cancellationToken = default)
    {
        SearchUserHealthConditionsRequest request = new(displayNameQuery, skip, take);
        ValidationResult validationResult = await _searchValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchUserHealthConditionsQuery query = request.MapToQuery(patientUserId);
        ErrorOr<SearchUserHealthConditionsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Checks whether a custom health condition name is still available for the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="customHealthConditionName">Custom name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result.</returns>
    [HttpGet("custom-name-availability")]
    [ProducesResponseType(typeof(CheckUserHealthConditionCustomNameAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CheckUserHealthConditionCustomNameAvailabilityResponse>> CheckCustomName(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "custom_health_condition_name")] string customHealthConditionName,
        CancellationToken cancellationToken = default)
    {
        CheckUserHealthConditionCustomNameAvailabilityRequest request = new(customHealthConditionName);
        ValidationResult validationResult = await _checkValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckUserHealthConditionCustomNameAvailabilityQuery query = request.MapToQuery(patientUserId);
        ErrorOr<CheckUserHealthConditionCustomNameAvailabilityResponse> result =
            await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds a health condition to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client add request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created health condition (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserHealthConditionDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserHealthConditionDto>> Add(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] AddUserHealthConditionRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _addValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddUserHealthConditionCommand command = request.MapToCommand(patientUserId);
        ErrorOr<UserHealthConditionDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates the note of a study patient's health condition.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Health condition row identifier.</param>
    /// <param name="request">Client note update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated health condition.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UserHealthConditionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserHealthConditionDto>> UpdateNote(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateUserHealthConditionNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateNoteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateUserHealthConditionNoteCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UserHealthConditionDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Removes a health condition from the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Health condition row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        RemoveUserHealthConditionCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
