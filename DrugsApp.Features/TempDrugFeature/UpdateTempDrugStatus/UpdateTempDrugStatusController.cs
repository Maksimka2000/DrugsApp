using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.UpdateTempDrugStatus;

/// <summary>
/// Request model for updating a temporary drug status.
/// </summary>
public sealed record UpdateTempDrugStatusRequest(
    TempDrugStatus Status,
    string? Note);

/// <summary>
/// Response containing updated temporary drug status.
/// </summary>
public sealed record UpdatedTempDrugStatusResponse(
    Guid Id,
    TempDrugStatus Status,
    string? Note);

/// <summary>
/// Command for updating a temporary drug status.
/// </summary>
public sealed record UpdateTempDrugStatusCommand(
    Guid TempDrugId,
    UpdateTempDrugStatusRequest Request) : IRequest<ErrorOr<UpdatedTempDrugStatusResponse>>;

/// <summary>
/// Controller for updating temporary drug status.
/// </summary>
[ApiController]
[Tags("Temp Drugs")]
[Route("api/temp-drugs")]
public sealed class UpdateTempDrugStatusController(
    IMediator _mediator,
    IValidator<UpdateTempDrugStatusRequest> _validator) : ControllerBase
{

    /// <summary>
    /// Updates the status of a temporary drug.
    /// </summary>
    /// <param name="id">Temporary drug id.</param>
    /// <param name="request">Update request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated status.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Temporary drug not found.</response>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<UpdatedTempDrugStatusResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTempDrugStatusRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateTempDrugStatusCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedTempDrugStatusResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            updated => Ok(updated),
            errors => errors.MapToProblem(HttpContext));
    }
}


