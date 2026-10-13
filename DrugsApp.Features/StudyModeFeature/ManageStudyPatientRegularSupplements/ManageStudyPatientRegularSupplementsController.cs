using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.ClearRegularSupplementHistory;
using DrugsApp.Features.RegularSupplementsFeature.CreateRegularSupplement;
using DrugsApp.Features.RegularSupplementsFeature.DeleteRegularSupplement;
using DrugsApp.Features.RegularSupplementsFeature.GetRegularSupplement;
using DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplements;
using DrugsApp.Features.RegularSupplementsFeature.ListRegularSupplementHistory;
using DrugsApp.Features.RegularSupplementsFeature.MoveRegularSupplementToHistory;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.UpdateRegularSupplement;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientRegularSupplements;

/// <summary>
/// Proxies the client regular supplement endpoints (live list, history, schedule changes) for a study patient; live-row caps
/// and duplicate checks run inside the unchanged client handlers.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/regular-supplements")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientRegularSupplementsController(
    IMediator _mediator,
    IValidator<ListRegularSupplementsRequest> _listValidator,
    IValidator<ListRegularSupplementHistoryRequest> _historyValidator,
    IValidator<CreateRegularSupplementRequest> _createValidator,
    IValidator<UpdateRegularSupplementRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's live (scheduled and active) regular supplement rows.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client filter and paging request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of live rows.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListRegularSupplementsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListRegularSupplementsResponse>> List(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] ListRegularSupplementsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _listValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListRegularSupplementsQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListRegularSupplementsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Returns one regular supplement row of the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The row details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetRegularSupplementResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetRegularSupplementResponse>> Get(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        GetRegularSupplementQuery query = new(patientUserId, id);
        ErrorOr<GetRegularSupplementResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds a regular supplement row to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created row (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRegularSupplementResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateRegularSupplementResponse>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateRegularSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateRegularSupplementCommand command = request.MapToCommand(patientUserId);
        ErrorOr<CreateRegularSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates the schedule of a study patient's regular supplement row.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated row.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UpdateRegularSupplementResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateRegularSupplementResponse>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateRegularSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateRegularSupplementCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UpdateRegularSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's regular supplement row.
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
        DeleteRegularSupplementCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Moves a study patient's live regular supplement row to history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the archived row.</returns>
    [HttpPost("{id:guid}/move-to-history")]
    [ProducesResponseType(typeof(MoveRegularSupplementToHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoveRegularSupplementToHistoryResponse>> MoveToHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        MoveRegularSupplementToHistoryCommand command = new(patientUserId, id);
        ErrorOr<MoveRegularSupplementToHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Lists the study patient's regular supplement history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client filter and paging request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of history rows.</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ListRegularSupplementHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListRegularSupplementHistoryResponse>> ListHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] ListRegularSupplementHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _historyValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListRegularSupplementHistoryQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListRegularSupplementHistoryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Clears the study patient's regular supplement history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of deleted history rows.</returns>
    [HttpDelete("history")]
    [ProducesResponseType(typeof(ClearRegularSupplementHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClearRegularSupplementHistoryResponse>> ClearHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken = default)
    {
        ClearRegularSupplementHistoryCommand command = new(patientUserId);
        ErrorOr<ClearRegularSupplementHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
