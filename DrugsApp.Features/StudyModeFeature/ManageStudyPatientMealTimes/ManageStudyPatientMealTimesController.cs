using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.MealTimeManagementFeature.CheckMealTimeAvailability;
using DrugsApp.Features.MealTimeManagementFeature.CreateMealTime;
using DrugsApp.Features.MealTimeManagementFeature.DeleteMealTime;
using DrugsApp.Features.MealTimeManagementFeature.ListMealTimes;
using DrugsApp.Features.MealTimeManagementFeature.RetrieveMealTime;
using DrugsApp.Features.MealTimeManagementFeature.Shared;
using DrugsApp.Features.MealTimeManagementFeature.UpdateMealTime;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientMealTimes;

/// <summary>
/// Proxies the client meal time (diet) endpoints for a study patient.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/meal-times")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientMealTimesController(
    IMediator _mediator,
    IValidator<CheckMealTimeAvailabilityRequest> _checkValidator,
    IValidator<CreateMealTimeRequest> _createValidator,
    IValidator<UpdateMealTimeRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's meal times.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The meal times.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListMealTimesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListMealTimesResponse>> List(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken = default)
    {
        ListMealTimesQuery query = new(patientUserId);
        ErrorOr<ListMealTimesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Returns one meal time of the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Meal time identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The meal time.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MealTimeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MealTimeDto>> Retrieve(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        RetrieveMealTimeQuery query = new(patientUserId, id);
        ErrorOr<MealTimeDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Checks whether a meal name or intake time is still available for the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="mealName">Optional meal name to check.</param>
    /// <param name="intakeTime">Optional intake time to check.</param>
    /// <param name="excludeId">Optional meal time excluded from the check (edit mode).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result.</returns>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(MealTimeAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MealTimeAvailabilityResponse>> CheckAvailability(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "meal_name")] string? mealName,
        [FromQuery(Name = "intake_time")] TimeOnly? intakeTime,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken = default)
    {
        CheckMealTimeAvailabilityRequest request = new(mealName, intakeTime, excludeId);
        ValidationResult validationResult = await _checkValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckMealTimeAvailabilityQuery query = request.MapToQuery(patientUserId);
        ErrorOr<MealTimeAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Creates a meal time for the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created meal time (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(MealTimeDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MealTimeDto>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateMealTimeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateMealTimeCommand command = request.MapToCommand(patientUserId);
        ErrorOr<MealTimeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates a study patient's meal time.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Meal time identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated meal time.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(MealTimeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MealTimeDto>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateMealTimeRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateMealTimeCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<MealTimeDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's meal time.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Meal time identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        DeleteMealTimeCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
