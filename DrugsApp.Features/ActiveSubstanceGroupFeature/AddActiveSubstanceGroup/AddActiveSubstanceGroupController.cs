using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.ActiveSubstanceFeature.Shared;
using DrugsApp.Features.ActiveSubstanceGroupFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.AddActiveSubstanceGroup;

/// <summary>
/// Request payload for creating an active substance group.
/// </summary>
public sealed record AddActiveSubstanceGroupRequest(
    string Name,
    IReadOnlyCollection<Guid> ActiveSubstanceIds);

/// <summary>
/// Response containing the created group snapshot.
/// </summary>
public sealed record CreatedActiveSubstanceGroupResponse(
    Guid Id,
    string Name,
    IReadOnlyCollection<ActiveSubstanceDto> Members);

/// <summary>
/// Command for creating an active substance group.
/// </summary>
public sealed record AddActiveSubstanceGroupCommand(
    AddActiveSubstanceGroupRequest Request) : IRequest<ErrorOr<CreatedActiveSubstanceGroupResponse>>;

/// <summary>
/// Controller for creating active substance groups.
/// </summary>
[ApiController]
[Tags("Active Substance Groups")]
[Route("api/active-substance-groups")]
public sealed class AddActiveSubstanceGroupController(
    IMediator _mediator,
    IValidator<AddActiveSubstanceGroupRequest> _validator) : ControllerBase
{
    public const string RouteName = "AddActiveSubstanceGroup";

    /// <summary>
    /// Creates a new active substance group.
    /// </summary>
    /// <param name="request">Group name and member identifiers.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Created active substance group.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">One or more active substances were not found.</response>
    /// <response code="409">Group name or member set conflicts with an existing group.</response>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreatedActiveSubstanceGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedActiveSubstanceGroupResponse>> Create(
        [FromBody] AddActiveSubstanceGroupRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddActiveSubstanceGroupCommand command = request.MapToCommand();
        ErrorOr<CreatedActiveSubstanceGroupResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: DrugsApp.Features.ActiveSubstanceGroupFeature.UpdateActiveSubstanceGroup.UpdateActiveSubstanceGroupController.RouteName,
                routeValues: new { id = response.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
