using DrugsApp.Features.AllergiesFeature.CheckFoodCustomNameAvailability;
using DrugsApp.Features.AllergiesFeature.CreateFoodAllergy;
using DrugsApp.Features.AllergiesFeature.DeleteFoodAllergy;
using DrugsApp.Features.AllergiesFeature.ListFoodAllergies;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.AllergiesFeature.UpdateFoodAllergyNote;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientFoodAllergies;

/// <summary>
/// Proxies the client food allergy endpoints for a study patient; per-user caps and duplicate checks run inside the
/// unchanged client handlers.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/allergies/foods")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientFoodAllergiesController(
    IMediator _mediator,
    IValidator<ListFoodAllergiesRequest> _listValidator,
    IValidator<CheckFoodCustomNameAvailabilityRequest> _checkValidator,
    IValidator<CreateFoodAllergyRequest> _createValidator,
    IValidator<UpdateAllergyNoteRequest> _updateNoteValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's food allergies.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="skip">Paging offset.</param>
    /// <param name="take">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of food allergies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListFoodAllergiesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListFoodAllergiesResponse>> List(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        ListFoodAllergiesRequest request = new(skip, take);
        ValidationResult validationResult = await _listValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListFoodAllergiesQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListFoodAllergiesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Checks whether a custom food name is still available for the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="customName">Custom food name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result.</returns>
    [HttpGet("custom-name-availability")]
    [ProducesResponseType(typeof(NameAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NameAvailabilityResponse>> CheckCustomName(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "custom_food_name")] string customName,
        CancellationToken cancellationToken = default)
    {
        CheckFoodCustomNameAvailabilityRequest request = new(customName);
        ValidationResult validationResult = await _checkValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckFoodCustomNameAvailabilityQuery query = request.MapToQuery(patientUserId);
        ErrorOr<NameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds a food allergy to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created allergy (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedFoodAllergyResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedFoodAllergyResponse>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateFoodAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateFoodAllergyCommand command = request.MapToCommand(patientUserId);
        ErrorOr<CreatedFoodAllergyResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates the note of a study patient's food allergy.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Allergy identifier.</param>
    /// <param name="request">Client note update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated allergy.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UpdatedFoodAllergyResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdatedFoodAllergyResponse>> UpdateNote(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateAllergyNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateNoteValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateFoodAllergyNoteCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UpdatedFoodAllergyResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's food allergy.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Allergy identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        DeleteFoodAllergyCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
