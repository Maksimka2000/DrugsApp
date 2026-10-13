using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.SaveStudyClinicCanvas;

/// <summary>
/// Desired canvas edge between a doctor node and a patient node.
/// </summary>
/// <param name="DoctorUserId">Doctor node user identifier.</param>
/// <param name="PatientUserId">Patient node user identifier.</param>
public sealed record StudyAssociationRequest(
    Guid DoctorUserId,
    Guid PatientUserId);

/// <summary>
/// Node coordinates to persist.
/// </summary>
/// <param name="UserId">Node user identifier.</param>
/// <param name="PositionX">Horizontal canvas coordinate.</param>
/// <param name="PositionY">Vertical canvas coordinate.</param>
public sealed record StudyNodePositionRequest(
    Guid UserId,
    double PositionX,
    double PositionY);

/// <summary>
/// Body of the canvas save request.
/// </summary>
/// <param name="Associations">Complete desired edge set; an empty list removes all edges (up to 1200 items).</param>
/// <param name="Positions">Node coordinates to persist (up to 80 items).</param>
public sealed record SaveStudyClinicCanvasRequest(
    IReadOnlyList<StudyAssociationRequest> Associations,
    IReadOnlyList<StudyNodePositionRequest> Positions);

/// <summary>
/// Command saving positions and the doctor-patient association set of a clinic.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="Request">Composed body.</param>
public sealed record SaveStudyClinicCanvasCommand(
    Guid ClinicId,
    SaveStudyClinicCanvasRequest Request) : IRequest<ErrorOr<Updated>>;

/// <summary>
/// Saves the study clinic canvas for teachers and admins.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class SaveStudyClinicCanvasController(
    IMediator _mediator,
    IValidator<SaveStudyClinicCanvasRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "SaveStudyClinicCanvas";

    /// <summary>
    /// Persists node positions and replaces the active doctor-patient associations (view-only supervision edges);
    /// last write wins.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="request">Associations and positions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Canvas saved.</response>
    /// <response code="400">Invalid payload or references to non-members.</response>
    /// <response code="404">Clinic was not found.</response>
    [HttpPut("{clinic_id:guid}/canvas", Name = RouteName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Save(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromBody] SaveStudyClinicCanvasRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        SaveStudyClinicCanvasCommand command = request.MapToCommand(clinicId);
        ErrorOr<Updated> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            _ => NoContent(),
            errors => errors.MapToProblem(HttpContext));
    }
}
