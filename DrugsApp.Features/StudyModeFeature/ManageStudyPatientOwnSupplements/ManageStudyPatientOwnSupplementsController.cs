using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.CheckOwnSupplementNameAvailability;
using DrugsApp.Features.RegularSupplementsFeature.CreateOwnSupplement;
using DrugsApp.Features.RegularSupplementsFeature.DeleteOwnSupplement;
using DrugsApp.Features.RegularSupplementsFeature.GetOwnSupplement;
using DrugsApp.Features.RegularSupplementsFeature.ListOwnSupplements;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using DrugsApp.Features.RegularSupplementsFeature.UpdateOwnSupplement;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientOwnSupplements;

/// <summary>
/// Proxies the client own-supplement (custom supplement mix) endpoints for a study patient.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/own-supplements")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientOwnSupplementsController(
    IMediator _mediator,
    IValidator<CheckOwnSupplementNameAvailabilityRequest> _checkValidator,
    IValidator<CreateOwnSupplementRequest> _createValidator,
    IValidator<UpdateOwnSupplementRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's own supplements.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The own supplements.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListOwnSupplementsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListOwnSupplementsResponse>> List(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken = default)
    {
        ListOwnSupplementsQuery query = new(patientUserId);
        ErrorOr<ListOwnSupplementsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Returns one own supplement of the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Own supplement identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The own supplement details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetOwnSupplementResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetOwnSupplementResponse>> Get(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        GetOwnSupplementQuery query = new(patientUserId, id);
        ErrorOr<GetOwnSupplementResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Checks whether an own-supplement name is still available for the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="name">Name to check.</param>
    /// <param name="excludeId">Optional own supplement excluded from the check (edit mode).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result.</returns>
    [HttpGet("validate-name-availability")]
    [ProducesResponseType(typeof(CheckOwnSupplementNameAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CheckOwnSupplementNameAvailabilityResponse>> CheckName(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "name")] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        CheckOwnSupplementNameAvailabilityRequest request = new(name);
        ValidationResult validationResult = await _checkValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckOwnSupplementNameAvailabilityQuery query = request.MapToQuery(patientUserId, excludeId);
        ErrorOr<CheckOwnSupplementNameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Creates an own supplement for the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created own supplement (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateOwnSupplementResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateOwnSupplementResponse>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateOwnSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateOwnSupplementCommand command = request.MapToCommand(patientUserId);
        ErrorOr<CreateOwnSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates a study patient's own supplement.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Own supplement identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated own supplement.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UpdateOwnSupplementResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateOwnSupplementResponse>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateOwnSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateOwnSupplementCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UpdateOwnSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's own supplement.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Own supplement identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deletion summary.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(DeleteOwnSupplementResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DeleteOwnSupplementResponse>> Delete(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        DeleteOwnSupplementCommand command = new(patientUserId, id);
        ErrorOr<DeleteOwnSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
