using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.HealthConditionFeature.CheckUserHealthConditionCustomNameAvailability;

/// <summary>
/// Request model for checking custom user health condition name availability.
/// </summary>
public sealed record CheckUserHealthConditionCustomNameAvailabilityRequest(
    string CustomHealthConditionName);

/// <summary>
/// Response indicating availability.
/// </summary>
public sealed record CheckUserHealthConditionCustomNameAvailabilityResponse(
    bool IsAvailable);

/// <summary>
/// Query for checking user health condition custom name availability.
/// </summary>
public sealed record CheckUserHealthConditionCustomNameAvailabilityQuery(
    Guid UserId,
    string NormalizedCustomHealthConditionName) : IRequest<ErrorOr<CheckUserHealthConditionCustomNameAvailabilityResponse>>;

/// <summary>
/// Controller for checking custom user health condition name availability.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("User Health Conditions")]
[Route("api/user-health-conditions/custom-name-availability")]
public sealed class CheckUserHealthConditionCustomNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckUserHealthConditionCustomNameAvailabilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for availability endpoint.
    /// </summary>
    public const string RouteName = "CheckUserHealthConditionCustomNameAvailability";

    /// <summary>
    /// Checks if a custom health condition name is available for the current user.
    /// </summary>
    [HttpGet(Name = RouteName)]
    [ProducesResponseType(typeof(CheckUserHealthConditionCustomNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckUserHealthConditionCustomNameAvailabilityResponse>> Check(
        [FromQuery(Name = "custom_health_condition_name")] string customHealthConditionName,
        CancellationToken cancellationToken = default)
    {
        CheckUserHealthConditionCustomNameAvailabilityRequest request = new(customHealthConditionName);

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

        CheckUserHealthConditionCustomNameAvailabilityQuery query = request.MapToQuery(userId);
        ErrorOr<CheckUserHealthConditionCustomNameAvailabilityResponse> result =
            await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
