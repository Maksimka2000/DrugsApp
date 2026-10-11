using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularDrugsFeature.ClearRegularDrugHistory;
using DrugsApp.Features.RegularDrugsFeature.CreateRegularDrug;
using DrugsApp.Features.RegularDrugsFeature.DeleteRegularDrug;
using DrugsApp.Features.RegularDrugsFeature.GetRegularDrug;
using DrugsApp.Features.RegularDrugsFeature.ListRegularDrugs;
using DrugsApp.Features.RegularDrugsFeature.ListRegularDrugHistory;
using DrugsApp.Features.RegularDrugsFeature.MoveRegularDrugToHistory;
using DrugsApp.Features.RegularDrugsFeature.UpdateRegularDrug;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientRegularDrugs;

/// <summary>
/// Proxies the client regular drug endpoints (live list, history, schedule changes) for a study patient; live-row caps
/// and duplicate checks run inside the unchanged client handlers.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/regular-drugs")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientRegularDrugsController(
    IMediator _mediator,
    IValidator<ListRegularDrugsRequest> _listValidator,
    IValidator<ListRegularDrugHistoryRequest> _historyValidator,
    IValidator<CreateRegularDrugRequest> _createValidator,
    IValidator<UpdateRegularDrugRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's live (scheduled and active) regular drug rows.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client filter and paging request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of live rows.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListRegularDrugsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListRegularDrugsResponse>> List(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] ListRegularDrugsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _listValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListRegularDrugsQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListRegularDrugsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Returns one regular drug row of the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The row details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetRegularDrugResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetRegularDrugResponse>> Get(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        GetRegularDrugQuery query = new(patientUserId, id);
        ErrorOr<GetRegularDrugResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds a regular drug row to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created row (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateRegularDrugResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateRegularDrugResponse>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateRegularDrugRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateRegularDrugCommand command = request.MapToCommand(patientUserId);
        ErrorOr<CreateRegularDrugResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates the schedule of a study patient's regular drug row.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated row.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UpdateRegularDrugResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateRegularDrugResponse>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateRegularDrugRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateRegularDrugCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UpdateRegularDrugResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's regular drug row.
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
        DeleteRegularDrugCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Moves a study patient's live regular drug row to history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Row identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of the archived row.</returns>
    [HttpPost("{id:guid}/move-to-history")]
    [ProducesResponseType(typeof(MoveRegularDrugToHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MoveRegularDrugToHistoryResponse>> MoveToHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        MoveRegularDrugToHistoryCommand command = new(patientUserId, id);
        ErrorOr<MoveRegularDrugToHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Lists the study patient's regular drug history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client filter and paging request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of history rows.</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ListRegularDrugHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListRegularDrugHistoryResponse>> ListHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] ListRegularDrugHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _historyValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListRegularDrugHistoryQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListRegularDrugHistoryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Clears the study patient's regular drug history.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of deleted history rows.</returns>
    [HttpDelete("history")]
    [ProducesResponseType(typeof(ClearRegularDrugHistoryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ClearRegularDrugHistoryResponse>> ClearHistory(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken = default)
    {
        ClearRegularDrugHistoryCommand command = new(patientUserId);
        ErrorOr<ClearRegularDrugHistoryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
