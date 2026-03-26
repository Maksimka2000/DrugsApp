using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.ActiveSubstanceFeature.Entities;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.Features.ActiveSubstanceFeature.UpdateActiveSubstance;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceFeature.AddOralMedicationIntakeGuidelines;



/// <summary>
/// Response containing created oral medication intake guidelines details.
/// </summary>
public sealed record CreatedOralMedicationIntakeGuidelinesResponse(
    Guid ActiveSubstanceId,
    OralMedicationIntakeGuidelinesDto OralMedicationIntakeGuidelines);

/// <summary>
/// Command for adding oral medication intake guidelines.
/// </summary>
public sealed record AddOralMedicationIntakeGuidelinesCommand(
    Guid ActiveSubstanceId,
    AddOralMedicationIntakeGuidelinesRequest Request) : IRequest<ErrorOr<CreatedOralMedicationIntakeGuidelinesResponse>>;

/// <summary>
/// Controller for adding oral medication intake guidelines.
/// </summary>
[ApiController]
[Tags("Active Substances")]
[Route("api/active-substances")]
public sealed class AddOralMedicationIntakeGuidelinesController(
    IMediator _mediator,
    IValidator<AddOralMedicationIntakeGuidelinesRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "AddOralMedicationIntakeGuidelines";

    /// <summary>
    /// Attaches oral medication intake guidelines to an active substance.
    /// </summary>
    /// <param name="id">Active substance unique identifier.</param>
    /// <param name="request">Guidelines payload to attach.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created guidelines snapshot.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Active substance not found.</response>
    /// <response code="409">Guidelines already exist for this active substance.</response>
    [HttpPost("{id:guid}/oral-medication-intake-guidelines", Name = RouteName)]
    [ProducesResponseType(typeof(CreatedOralMedicationIntakeGuidelinesResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedOralMedicationIntakeGuidelinesResponse>> Create(
        [FromRoute] Guid id,
        [FromBody] AddOralMedicationIntakeGuidelinesRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddOralMedicationIntakeGuidelinesCommand command = request.MapToCommand(id);
        ErrorOr<CreatedOralMedicationIntakeGuidelinesResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            created => CreatedAtRoute(
                routeName: UpdateActiveSubstanceController.RouteName,
                routeValues: new { id = created.ActiveSubstanceId },
                value: created),
            errors => errors.MapToProblem(HttpContext));
    }
}


