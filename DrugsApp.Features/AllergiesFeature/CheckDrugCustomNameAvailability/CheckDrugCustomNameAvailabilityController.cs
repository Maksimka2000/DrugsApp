using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Domain.AuthFeature;
using DrugsApp.Features.AllergiesFeature.Shared;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.AllergiesFeature.CheckDrugCustomNameAvailability;

/// <summary>
/// Request model for checking drug custom name availability.
/// </summary>
public sealed record CheckDrugCustomNameAvailabilityRequest(string CustomDrugName);

/// <summary>
/// Query for checking drug custom name availability.
/// </summary>
public sealed record CheckDrugCustomNameAvailabilityQuery(
    Guid UserId,
    string CustomDrugName) : IRequest<ErrorOr<NameAvailabilityResponse>>;

/// <summary>
/// Controller for checking drug custom name availability.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class CheckDrugCustomNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckDrugCustomNameAvailabilityRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for checking drug custom name availability.
    /// </summary>
    public const string RouteName = "CheckDrugCustomNameAvailability";

    /// <summary>
    /// Checks whether a custom drug name is available for the current user.
    /// </summary>
    [HttpGet("drugs/custom-name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(NameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NameAvailabilityResponse>> Check(
        [FromQuery(Name = "custom_drug_name")] string customDrugName,
        CancellationToken cancellationToken = default)
    {
        CheckDrugCustomNameAvailabilityRequest request = new(customDrugName);

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

        CheckDrugCustomNameAvailabilityQuery query = request.MapToQuery(userId);
        ErrorOr<NameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<NameAvailabilityResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
