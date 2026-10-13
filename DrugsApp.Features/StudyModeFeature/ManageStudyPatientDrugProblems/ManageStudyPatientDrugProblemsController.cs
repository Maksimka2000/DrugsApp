using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugProblemsFeature.CheckDrugProblemCustomNameAvailability;
using DrugsApp.Features.DrugProblemsFeature.CreateDrugProblem;
using DrugsApp.Features.DrugProblemsFeature.DeleteDrugProblem;
using DrugsApp.Features.DrugProblemsFeature.SearchDrugProblems;
using DrugsApp.Features.DrugProblemsFeature.UpdateDrugProblem;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientDrugProblems;

/// <summary>
/// Proxies the client drug problem endpoints for a study patient.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/drug-problems")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientDrugProblemsController(
    IMediator _mediator,
    IValidator<SearchDrugProblemsRequest> _searchValidator,
    IValidator<CheckDrugProblemCustomNameAvailabilityRequest> _checkValidator,
    IValidator<CreateDrugProblemRequest> _createValidator,
    IValidator<UpdateDrugProblemRequest> _updateValidator) : ControllerBase
{
    /// <summary>
    /// Lists the study patient's drug problems.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client paging request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A page of drug problems.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SearchDrugProblemsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SearchDrugProblemsResponse>> Search(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] SearchDrugProblemsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _searchValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SearchDrugProblemsQuery query = request.MapToQuery(patientUserId);
        ErrorOr<SearchDrugProblemsResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Checks whether a custom drug name is still available for the study patient's drug problems.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client availability request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Availability result.</returns>
    [HttpGet("custom-name-availability")]
    [ProducesResponseType(typeof(DrugProblemCustomNameAvailabilityDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DrugProblemCustomNameAvailabilityDto>> CheckCustomName(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromQuery] CheckDrugProblemCustomNameAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _checkValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckDrugProblemCustomNameAvailabilityQuery query = request.MapToQuery(patientUserId);
        ErrorOr<DrugProblemCustomNameAvailabilityDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Adds a drug problem to the study patient.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>201 with the created drug problem (no location header).</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateDrugProblemResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateDrugProblemResponse>> Create(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] CreateDrugProblemRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CreateDrugProblemCommand command = request.MapToCommand(patientUserId);
        ErrorOr<CreateDrugProblemResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created, response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates a study patient's drug problem.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Drug problem identifier.</param>
    /// <param name="request">Client update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated drug problem.</returns>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UpdateDrugProblemResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateDrugProblemResponse>> Update(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        [FromBody] UpdateDrugProblemRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateDrugProblemCommand command = request.MapToCommand(patientUserId, id);
        ErrorOr<UpdateDrugProblemResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Deletes a study patient's drug problem.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="id">Drug problem identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>204 on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        DeleteDrugProblemCommand command = new(patientUserId, id);
        ErrorOr<Success> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
