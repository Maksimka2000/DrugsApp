using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.UpdateOwnSupplement;

/// <summary>
/// Request model for updating an own supplement compound.
/// </summary>
public sealed record UpdateOwnSupplementRequest(
    string Name,
    IReadOnlyCollection<Guid> SupplementIds);

/// <summary>
/// Command for updating an own supplement compound.
/// </summary>
public sealed record UpdateOwnSupplementCommand(
    Guid UserId,
    Guid Id,
    UpdateOwnSupplementRequest Request) : IRequest<ErrorOr<UpdateOwnSupplementResponse>>;

/// <summary>
/// Response containing the updated own supplement compound.
/// </summary>
public sealed record UpdateOwnSupplementResponse(OwnSupplementDetailsDto Item);

/// <summary>
/// Controller for updating own supplement compounds.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Own Supplements")]
[Route("api/own-supplements")]
public sealed class UpdateOwnSupplementController(
    IMediator _mediator,
    IValidator<UpdateOwnSupplementRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for updating an own supplement compound.
    /// </summary>
    public const string RouteName = "UpdateOwnSupplement";

    /// <summary>
    /// Updates an own supplement compound owned by the authenticated user.
    /// </summary>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdateOwnSupplementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdateOwnSupplementResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateOwnSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return ValidationProblem("id must not be empty.");
        }

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Unauthorized();
        }

        UpdateOwnSupplementCommand command = request.MapToCommand(userId, id);
        ErrorOr<UpdateOwnSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<UpdateOwnSupplementResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
