using DrugsApp.Features.AllergiesFeature.GetAllergySummary;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.AllergiesFeature.UpsertAllergySummary;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.ManageStudyPatientAllergySummary;

/// <summary>
/// Proxies the client allergy summary endpoints for a study patient.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/allergies/summary")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class ManageStudyPatientAllergySummaryController(
    IMediator _mediator,
    IValidator<UpsertAllergySummaryRequest> _upsertValidator) : ControllerBase
{
    /// <summary>
    /// Returns the study patient's allergy summary (created lazily by the client handler).
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The allergy summary.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(AllergySummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AllergySummaryResponse>> Get(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        CancellationToken cancellationToken)
    {
        GetAllergySummaryQuery query = new(patientUserId);
        ErrorOr<AllergySummaryResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Creates or updates the study patient's allergy summary.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="request">Client upsert request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored allergy summary.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(AllergySummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AllergySummaryResponse>> Upsert(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromBody] UpsertAllergySummaryRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _upsertValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpsertAllergySummaryCommand command = request.MapToCommand(patientUserId);
        ErrorOr<AllergySummaryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
