using DrugsApp.Domain.StudyModeFeature.Enums;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.GenerateStudyPatientRandomData;

/// <summary>
/// Body of the random data request.
/// </summary>
/// <param name="Count">Number of random items to add (1–10).</param>
public sealed record GenerateStudyPatientRandomDataBody(int Count);

/// <summary>
/// Validated input composed from the route section token and the body.
/// </summary>
/// <param name="Section">Section token, for example <c>drug-allergies</c> or <c>regular-food</c>.</param>
/// <param name="Count">Number of random items to add (1–10).</param>
public sealed record GenerateStudyPatientRandomDataRequest(
    string Section,
    int Count);

/// <summary>
/// Command adding random sample items to one medical section of a study patient.
/// </summary>
/// <param name="PatientUserId">Study patient user identifier.</param>
/// <param name="Section">Parsed section.</param>
/// <param name="Count">Number of random items to add.</param>
public sealed record GenerateStudyPatientRandomDataCommand(
    Guid PatientUserId,
    StudyMedicalSection Section,
    int Count) : IRequest<ErrorOr<StudyRandomDataResponse>>;

/// <summary>
/// Fills a study patient's medical section with random catalog-based sample data for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode - Patient Medical Data")]
[Route("api/admin/study/clinics/{clinic_id:guid}/patients/{patient_id:guid}/random-data")]
[ServiceFilter(typeof(StudyPatientScopeFilter), Order = 1)]
public sealed class GenerateStudyPatientRandomDataController(
    IMediator _mediator,
    IValidator<GenerateStudyPatientRandomDataRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "GenerateStudyPatientRandomData";

    /// <summary>
    /// Adds up to <c>count</c> random, ongoing and non-duplicate items to the section; all items are committed
    /// together.
    /// </summary>
    /// <param name="patientUserId">Study patient user identifier.</param>
    /// <param name="section">Section token: <c>drug-allergies</c>, <c>supplement-allergies</c>, <c>food-allergies</c>, <c>other-allergies</c>, <c>health-conditions</c>, <c>drug-problems</c>, <c>regular-drugs</c>, <c>regular-supplements</c>, <c>own-supplements</c>, <c>regular-food</c>, <c>meal-times</c>.</param>
    /// <param name="body">Requested count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Items generated; see <c>createdCount</c> and <c>limitReached</c>.</response>
    /// <response code="400">Unknown section or count outside 1–10.</response>
    /// <response code="404">The patient is not a member of the clinic.</response>
    /// <response code="409">The section already holds the maximum number of items.</response>
    [HttpPost("{section}", Name = RouteName)]
    [ProducesResponseType(typeof(StudyRandomDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudyRandomDataResponse>> Generate(
        [FromRoute(Name = "patient_id")] Guid patientUserId,
        [FromRoute(Name = "section")] string section,
        [FromBody] GenerateStudyPatientRandomDataBody body,
        CancellationToken cancellationToken)
    {
        GenerateStudyPatientRandomDataRequest request = body.MapToRequest(section);
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        GenerateStudyPatientRandomDataCommand command = request.MapToCommand(patientUserId);
        ErrorOr<StudyRandomDataResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
