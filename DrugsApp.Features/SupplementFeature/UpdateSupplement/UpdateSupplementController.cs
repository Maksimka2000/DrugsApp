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

namespace DrugsApp.Features.SupplementFeature.UpdateSupplement;

/// <summary>
/// Request model for updating a supplement.
/// </summary>
public sealed record UpdateSupplementRequest(
    string? Name,
    IReadOnlyCollection<string>? Aliases);

/// <summary>
/// Command for updating supplement fields.
/// </summary>
public sealed record UpdateSupplementCommand(
    Guid SupplementId,
    UpdateSupplementRequest Request) : IRequest<ErrorOr<UpdatedSupplementResponse>>;

/// <summary>
/// Response after updating a supplement.
/// </summary>
public sealed record UpdatedSupplementResponse(
    SupplementDto Supplement);

/// <summary>
/// Controller for updating supplements.
/// </summary>
[ApiController]
[Tags("Supplements")]
[Route("api/supplements")]
public sealed class UpdateSupplementController(
    IMediator _mediator,
    IValidator<UpdateSupplementRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateSupplement";

    /// <summary>
    /// Updates supplement name and aliases.
    /// </summary>
    /// <param name="id">Supplement identifier.</param>
    /// <param name="request">Patch payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns updated supplement.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Supplement not found.</response>
    /// <response code="409">Name conflict.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedSupplementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedSupplementResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSupplementRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateSupplementCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
