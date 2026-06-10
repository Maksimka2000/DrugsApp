using System;
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

namespace DrugsApp.Features.RegularSupplementsFeature.CheckOwnSupplementNameAvailability;

/// <summary>
/// Request model for checking own supplement name availability.
/// </summary>
public sealed record CheckOwnSupplementNameAvailabilityRequest(string Name);

/// <summary>
/// Query for checking own supplement name availability.
/// </summary>
public sealed record CheckOwnSupplementNameAvailabilityQuery(
    Guid UserId,
    string NormalizedName,
    Guid? ExcludeId) : IRequest<ErrorOr<CheckOwnSupplementNameAvailabilityResponse>>;

/// <summary>
/// Controller for checking own supplement name availability.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Own Supplements")]
[Route("api/own-supplements/validate-name-availability")]
public sealed class CheckOwnSupplementNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckOwnSupplementNameAvailabilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for checking own supplement name availability.
    /// </summary>
    public const string RouteName = "CheckOwnSupplementNameAvailability";

    /// <summary>
    /// Checks whether an own supplement name is available for the authenticated user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(CheckOwnSupplementNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckOwnSupplementNameAvailabilityResponse>> Check(
        [FromQuery(Name = "name")] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        CheckOwnSupplementNameAvailabilityRequest request = new(name);

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

        CheckOwnSupplementNameAvailabilityQuery query = request.MapToQuery(userId, excludeId);
        ErrorOr<CheckOwnSupplementNameAvailabilityResponse> result =
            await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
