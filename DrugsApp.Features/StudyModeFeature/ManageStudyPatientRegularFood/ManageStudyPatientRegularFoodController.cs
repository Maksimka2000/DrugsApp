using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularFoodFeature.ClearRegularFoodHistory;
using DrugsApp.Features.RegularFoodFeature.CreateRegularFood;
using DrugsApp.Features.RegularFoodFeature.DeleteRegularFood;
using DrugsApp.Features.RegularFoodFeature.GetRegularFood;
using DrugsApp.Features.RegularFoodFeature.ListRegularFood;
using DrugsApp.Features.RegularFoodFeature.ListRegularFoodHistory;
using DrugsApp.Features.RegularFoodFeature.MoveRegularFoodToHistory;
using DrugsApp.Features.RegularFoodFeature.UpdateRegularFoodSchedule;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientRegularFood;

/// <summary>
/// Proxies the client regular food endpoints (live list, history, schedule changes) for a study patient; live-row caps
/// and duplicate checks run inside the unchanged client handlers.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/regular-food")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientRegularFoodController(
    IMediator _mediator,
    IValidator<ListRegularFoodRequest> _listValidator,
    IValidator<ListRegularFoodHistoryRequest> _historyValidator,
    IValidator<CreateRegularFoodRequest> _createValidator,
    IValidator<UpdateRegularFoodScheduleRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's live (scheduled and active) regular food rows.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client filter and paging request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of live rows.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListRegularFoodResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListRegularFoodResponse>> List(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] ListRegularFoodRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _listValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListRegularFoodQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListRegularFoodResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Returns one regular food row of the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The row details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetRegularFoodResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetRegularFoodResponse>> Get(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        GetRegularFoodQuery query = new(patientUserId, id);
        ErrorOr<GetRegularFoodResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds a regular food row to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created row (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRegularFoodResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateRegularFoodResponse>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateRegularFoodRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateRegularFoodCommand command = request.MapToCommand(patientUserId);
        ErrorOr<CreateRegularFoodResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates the schedule of a study patient's regular food row.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated row.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UpdateRegularFoodScheduleResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateRegularFoodScheduleResponse>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateRegularFoodScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateRegularFoodScheduleCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UpdateRegularFoodScheduleResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's regular food row.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        DeleteRegularFoodCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Moves a study patient's live regular food row to history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the archived row.</returns>
    [HttpPost("{id:guid}/move-to-history")]
    [ProducesResponseType(typeof(MoveRegularFoodToHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoveRegularFoodToHistoryResponse>> MoveToHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        MoveRegularFoodToHistoryCommand command = new(patientUserId, id);
        ErrorOr<MoveRegularFoodToHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Lists the study patient's regular food history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client filter and paging request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of history rows.</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ListRegularFoodHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListRegularFoodHistoryResponse>> ListHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] ListRegularFoodHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _historyValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListRegularFoodHistoryQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListRegularFoodHistoryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Clears the study patient's regular food history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of deleted history rows.</returns>
    [HttpDelete("history")]
    [ProducesResponseType(typeof(ClearRegularFoodHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClearRegularFoodHistoryResponse>> ClearHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken = default)
    {
        ClearRegularFoodHistoryCommand command = new(patientUserId);
        ErrorOr<ClearRegularFoodHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
