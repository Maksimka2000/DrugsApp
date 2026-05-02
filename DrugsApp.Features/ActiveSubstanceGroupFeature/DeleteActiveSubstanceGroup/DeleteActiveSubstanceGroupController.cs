using System;
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

namespace DrugsApp.Features.ActiveSubstanceGroupFeature.DeleteActiveSubstanceGroup;

/// <summary>
/// Request carrying the group identifier to delete.
/// </summary>
public sealed record DeleteActiveSubstanceGroupRequest(Guid Id);

/// <summary>
/// Response confirming the deleted group.
/// </summary>
public sealed record DeleteActiveSubstanceGroupResponse(Guid Id);

/// <summary>
/// Command for deleting an active substance group.
/// </summary>
public sealed record DeleteActiveSubstanceGroupCommand(Guid GroupId) : IRequest<ErrorOr<DeleteActiveSubstanceGroupResponse>>;

/// <summary>
/// Controller for deleting active substance groups.
/// </summary>
[ApiController]
[Tags("Active Substance Groups")]
[Route("api/active-substance-groups")]
public sealed class DeleteActiveSubstanceGroupController(
    IMediator _mediator,
    IValidator<DeleteActiveSubstanceGroupRequest> _validator) : ControllerBase
{
    public const string RouteName = "DeleteActiveSubstanceGroup";

    /// <summary>
    /// Deletes an active substance group.
    /// </summary>
    /// <param name="id">Group identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Group deletion confirmed.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">Group not found.</response>
    [HttpDelete("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(DeleteActiveSubstanceGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteActiveSubstanceGroupResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        DeleteActiveSubstanceGroupRequest request = new(id);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        DeleteActiveSubstanceGroupCommand command = request.MapToCommand();
        ErrorOr<DeleteActiveSubstanceGroupResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
