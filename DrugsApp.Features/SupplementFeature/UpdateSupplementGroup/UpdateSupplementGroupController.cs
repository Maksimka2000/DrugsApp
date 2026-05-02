using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupplementFeature.UpdateSupplementGroup;

/// <summary>
/// Request model for updating a supplement group.
/// </summary>
public sealed record UpdateSupplementGroupRequest(
    string? Name,
    IReadOnlyCollection<Guid>? SupplementIds);

/// <summary>
/// Command for updating supplement group fields.
/// </summary>
public sealed record UpdateSupplementGroupCommand(
    Guid SupplementGroupId,
    UpdateSupplementGroupRequest Request) : IRequest<ErrorOr<UpdatedSupplementGroupResponse>>;

/// <summary>
/// Response after updating supplement group.
/// </summary>
public sealed record UpdatedSupplementGroupResponse(
    SupplementGroupDto SupplementGroup);

/// <summary>
/// Controller for updating supplement groups.
/// </summary>
[ApiController]
[Tags("SupplementGroups")]
[Route("api/supplement-groups")]
public sealed class UpdateSupplementGroupController(
    IMediator mediator,
    IValidator<UpdateSupplementGroupRequest> validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating supplement groups.
    /// </summary>
    public const string RouteName = "UpdateSupplementGroup";

    /// <summary>
    /// Updates supplement group name and members.
    /// </summary>
    /// <param name="id">Supplement group identifier.</param>
    /// <param name="request">Patch payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns updated group.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Group not found.</response>
    /// <response code="409">Name conflict.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedSupplementGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedSupplementGroupResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSupplementGroupRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateSupplementGroupCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedSupplementGroupResponse> result = await mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
