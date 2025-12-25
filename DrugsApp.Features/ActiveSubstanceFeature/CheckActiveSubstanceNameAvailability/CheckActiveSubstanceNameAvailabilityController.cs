using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.ActiveSubstanceFeature.CheckActiveSubstanceNameAvailability;

/// <summary>
/// Request model for active substance name availability query parameters.
/// </summary>
public sealed record CheckActiveSubstanceNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Response indicating whether an active substance name is available.
/// </summary>
public sealed record ActiveSubstanceNameAvailabilityResponse(
    bool IsAvailable,
    Guid? ConflictingResourceId);

/// <summary>
/// Query for checking active substance name availability.
/// </summary>
public sealed record CheckActiveSubstanceNameAvailabilityQuery(
    string Name,
    Guid? ExcludeId) : IRequest<ErrorOr<ActiveSubstanceNameAvailabilityResponse>>;

/// <summary>
/// Controller for checking active substance name availability.
/// </summary>
[ApiController]
[Tags("Active Substances")]
[Route("api/active-substances")]
public sealed class CheckActiveSubstanceNameAvailabilityController(
    IMediator mediator,
    IValidator<CheckActiveSubstanceNameAvailabilityRequest> validator) : ControllerBase
{
    public const string RouteName = "CheckActiveSubstanceNameAvailability";

    private readonly IMediator _mediator = mediator;
    private readonly IValidator<CheckActiveSubstanceNameAvailabilityRequest> _validator = validator;

    /// <summary>
    /// Checks whether the provided active substance name is available.
    /// </summary>
    /// <param name="name">Name to check for availability.</param>
    /// <param name="excludeId">Optional active substance id to exclude.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns availability information.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet("name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(ActiveSubstanceNameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ActiveSubstanceNameAvailabilityResponse>> Check(
        [FromQuery] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken)
    {
        CheckActiveSubstanceNameAvailabilityRequest request = new(name, excludeId);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckActiveSubstanceNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<ActiveSubstanceNameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


