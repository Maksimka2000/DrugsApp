using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.StudyModeFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.StudyModeFeature.UpdateStudyClinicVisibility;

/// <summary>
/// Body of the clinic visibility change request.
/// </summary>
/// <param name="Visibility"><c>public</c> (every study mode admin) or <c>private</c> (creator only).</param>
public sealed record UpdateStudyClinicVisibilityRequest(string Visibility);

/// <summary>
/// Command changing the visibility of a study clinic.
/// </summary>
/// <param name="ClinicId">Clinic identifier.</param>
/// <param name="CallerUserId">Calling admin user; must be the clinic creator.</param>
/// <param name="Request">Composed body.</param>
public sealed record UpdateStudyClinicVisibilityCommand(
    Guid ClinicId,
    Guid CallerUserId,
    UpdateStudyClinicVisibilityRequest Request) : IRequest<ErrorOr<StudyClinicDto>>;

/// <summary>
/// Lets the creator of a study clinic switch it between public and private.
/// </summary>
[ApiController]
[ServiceFilter(typeof(StudyClinicAccessFilter), Order = 0)]
[Tags("Study Mode")]
[Route("api/admin/study/clinics")]
public sealed class UpdateStudyClinicVisibilityController(
    IMediator _mediator,
    IValidator<UpdateStudyClinicVisibilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateStudyClinicVisibility";

    /// <summary>
    /// Changes the clinic visibility; only the clinic creator may do it.
    /// </summary>
    /// <param name="clinicId">Clinic identifier.</param>
    /// <param name="request">New visibility.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated clinic header.</response>
    /// <response code="400">Invalid visibility value.</response>
    /// <response code="403">Caller is not the clinic creator.</response>
    /// <response code="404">Clinic was not found or is private to another user.</response>
    [HttpPut("{clinic_id:guid}/visibility", Name = RouteName)]
    [ProducesResponseType(typeof(StudyClinicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyClinicDto>> Update(
        [FromRoute(Name = "clinic_id")] Guid clinicId,
        [FromBody] UpdateStudyClinicVisibilityRequest request,
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

        UpdateStudyClinicVisibilityCommand command = request.MapToCommand(clinicId, callerUserId);
        ErrorOr<StudyClinicDto> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
