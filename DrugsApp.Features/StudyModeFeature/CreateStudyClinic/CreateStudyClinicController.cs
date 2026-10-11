using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.GetStudyClinicCanvas;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.CreateStudyClinic;

/// <summary>
/// Body of the study clinic creation request.
/// </summary>
/// <param name="Name">Clinic name (3–120 characters after trimming).</param>
/// <param name="ScenarioDescription">Optional scenario text (up to 2000 characters).</param>
/// <param name="Visibility">Optional <c>public</c> or <c>private</c>; defaults to <c>private</c> (creator only).</param>
public sealed record CreateStudyClinicRequest(
    string Name,
    string? ScenarioDescription,
    string? Visibility = null);

/// <summary>
/// Command creating a new empty study clinic.
/// </summary>
/// <param name="CreatedByUserId">Calling admin user identifier.</param>
/// <param name="Request">Composed body.</param>
public sealed record CreateStudyClinicCommand(
    Guid CreatedByUserId,
    CreateStudyClinicRequest Request) : IRequest<ErrorOr<StudyClinicDto>>;

/// <summary>
/// Creates study clinics for teachers and admins.
/// </summary>
[ApiController]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class CreateStudyClinicController(
    IMediator _mediator,
    IValidator<CreateStudyClinicRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "CreateStudyClinic";

    /// <summary>
    /// Creates a study clinic with a unique (case-insensitive) name.
    /// </summary>
    /// <param name="request">Clinic name and optional scenario description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Clinic created; location points at the canvas route.</response>
    /// <response code="400">Invalid payload.</response>
    /// <response code="409">A clinic with the same name already exists.</response>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(StudyClinicDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StudyClinicDto>> Create(
        [FromBody] CreateStudyClinicRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        if (!User.TryGetUserId(out Guid callerUserId))
        {
            return Unauthorized();
        }

        CreateStudyClinicCommand command = request.MapToCommand(callerUserId);
        ErrorOr<StudyClinicDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: GetStudyClinicCanvasController.RouteName,
                routeValues: new { clinic_id = response.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
