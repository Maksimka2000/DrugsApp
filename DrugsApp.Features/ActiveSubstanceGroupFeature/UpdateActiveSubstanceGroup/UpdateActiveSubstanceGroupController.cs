using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup;

/// <summary>
/// Request payload for updating an active substance group.
/// </summary>
public sealed record UpdateActiveSubstanceGroupRequest(
    string? Name,
    IReadOnlyCollection<Guid>? ActiveSubstanceIds);

/// <summary>
/// Response containing the updated group snapshot.
/// </summary>
public sealed record UpdatedActiveSubstanceGroupResponse(
    Guid Id,
    string Name,
    IReadOnlyCollection<ActiveSubstanceDto> Members);

/// <summary>
/// Command for updating an active substance group.
/// </summary>
public sealed record UpdateActiveSubstanceGroupCommand(
    Guid GroupId,
    UpdateActiveSubstanceGroupRequest Request) : IRequest<ErrorOr<UpdatedActiveSubstanceGroupResponse>>;

/// <summary>
/// Controller for updating active substance groups.
/// </summary>
[ApiController]
[Tags("Active Substance Groups")]
[Route("api/active-substance-groups")]
public sealed class UpdateActiveSubstanceGroupController(
    IMediator _mediator,
    IValidator<UpdateActiveSubstanceGroupRequest> _validator) : ControllerBase
{
    public const string RouteName = "UpdateActiveSubstanceGroup";

    /// <summary>
    /// Updates group name and/or membership set.
    /// </summary>
    /// <param name="id">Group identifier.</param>
    /// <param name="request">Patch payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Updated group representation.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">Group not found.</response>
    /// <response code="409">Conflicts on name or member set.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedActiveSubstanceGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedActiveSubstanceGroupResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateActiveSubstanceGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateActiveSubstanceGroupCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedActiveSubstanceGroupResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
