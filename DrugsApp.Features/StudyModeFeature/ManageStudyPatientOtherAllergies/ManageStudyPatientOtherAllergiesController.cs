using DrugsApp.Features.AllergiesFeature.CheckOtherAllergyNameAvailability;
using DrugsApp.Features.AllergiesFeature.CreateOtherAllergy;
using DrugsApp.Features.AllergiesFeature.DeleteOtherAllergy;
using DrugsApp.Features.AllergiesFeature.ListOtherAllergies;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.AllergiesFeature.UpdateOtherAllergy;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientOtherAllergies;

/// <summary>
/// Proxies the client other (environmental) allergy endpoints for a study patient.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/allergies/other-allergies")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientOtherAllergiesController(
    IMediator _mediator,
    IValidator<ListOtherAllergiesRequest> _listValidator,
    IValidator<CheckOtherAllergyNameAvailabilityRequest> _checkValidator,
    IValidator<CreateOtherAllergyRequest> _createValidator,
    IValidator<UpdateOtherAllergyRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's other allergies.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="skip">Paging offset.</param>
    /// <param name="take">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of other allergies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ListOtherAllergiesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListOtherAllergiesResponse>> List(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        ListOtherAllergiesRequest request = new(skip, take);
        ValidationResult validationResult = await _listValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        ListOtherAllergiesQuery query = request.MapToQuery(patientUserId);
        ErrorOr<ListOtherAllergiesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Checks whether an other-allergy name is still available for the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="allergyName">Allergy name to check.</param>
    /// <param name="excludeId">Optional allergy id excluded from the check (edit mode).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result.</returns>
    [HttpGet("name-availability")]
    [ProducesResponseType(typeof(NameAvailabilityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NameAvailabilityResponse>> CheckName(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery(Name = "name")] string allergyName,
        [FromQuery(Name = "exclude_id")] Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        CheckOtherAllergyNameAvailabilityRequest request = new(allergyName, excludeId);
        ValidationResult validationResult = await _checkValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckOtherAllergyNameAvailabilityQuery query = request.MapToQuery(patientUserId);
        ErrorOr<NameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds an other allergy to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created allergy (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreatedOtherAllergyResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatedOtherAllergyResponse>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateOtherAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateOtherAllergyCommand command = request.MapToCommand(patientUserId);
        ErrorOr<CreatedOtherAllergyResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates a study patient's other allergy.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Allergy identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated allergy.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UpdatedOtherAllergyResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdatedOtherAllergyResponse>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateOtherAllergyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateOtherAllergyCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UpdatedOtherAllergyResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's other allergy.
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
        DeleteOtherAllergyCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
