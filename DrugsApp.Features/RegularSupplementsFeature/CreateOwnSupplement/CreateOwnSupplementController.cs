using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.RegularSupplementsFeature.GetOwnSupplement;
using DrugsApp.Features.RegularSupplementsFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.RegularSupplementsFeature.CreateOwnSupplement;

/// <summary>
/// Request model for creating an own supplement compound.
/// </summary>
public sealed record CreateOwnSupplementRequest(
    string Name,
    IReadOnlyCollection<Guid> SupplementIds);

/// <summary>
/// Command for creating an own supplement compound.
/// </summary>
public sealed record CreateOwnSupplementCommand(
    Guid UserId,
    CreateOwnSupplementRequest Request) : IRequest<ErrorOr<CreateOwnSupplementResponse>>;

/// <summary>
/// Response containing the created own supplement compound.
/// </summary>
public sealed record CreateOwnSupplementResponse(OwnSupplementDetailsDto Item);

/// <summary>
/// Controller for creating own supplement compounds.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Own Supplements")]
[Route("api/own-supplements")]
public sealed class CreateOwnSupplementController(
    IMediator _mediator,
    IValidator<CreateOwnSupplementRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for creating an own supplement compound.
    /// </summary>
    public const string RouteName = "CreateOwnSupplement";

    /// <summary>
    /// Creates a new own supplement compound for the authenticated user.
    /// </summary>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreateOwnSupplementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateOwnSupplementResponse>> Create(
        [FromBody] CreateOwnSupplementRequest request,
        CancellationToken cancellationToken = default)
    {
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

        CreateOwnSupplementCommand command = request.MapToCommand(userId);
        ErrorOr<CreateOwnSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<CreateOwnSupplementResponse>>(
            response => CreatedAtRoute(
                GetOwnSupplementController.RouteName,
                new { id = response.Item.Id },
                response),
            errors => errors.MapToProblem(HttpContext));
    }
}
