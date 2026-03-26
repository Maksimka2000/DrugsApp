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

namespace DrugsApp.Features.PharmaceuticalFormFeature.CheckPharmaceuticalFormNameAvailability;

/// <summary>
/// Request model for pharmaceutical form name availability query parameters.
/// </summary>
public sealed record CheckPharmaceuticalFormNameAvailabilityRequest(
    string Name,
    Guid? ExcludeId);

/// <summary>
/// Response indicating whether a pharmaceutical form name is available.
/// </summary>
public sealed record PharmaceuticalFormNameAvailabilityDto(
    bool IsAvailable,
    Guid? ConflictingResourceId);

/// <summary>
/// Query for checking pharmaceutical form name availability.
/// </summary>
public sealed record CheckPharmaceuticalFormNameAvailabilityQuery(
    string Name,
    Guid? ExcludeId) : IRequest<ErrorOr<PharmaceuticalFormNameAvailabilityDto>>;

/// <summary>
/// Controller for checking pharmaceutical form name availability.
/// </summary>
[ApiController]
[Tags("Pharmaceutical Forms")]
[Route("api/pharmaceutical-forms")]
public sealed class CheckPharmaceuticalFormNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckPharmaceuticalFormNameAvailabilityRequest> _validator) : ControllerBase
{
    public const string RouteName = "CheckPharmaceuticalFormNameAvailability";

    /// <summary>
    /// Checks whether the provided pharmaceutical form name is available.
    /// </summary>
    /// <param name="name">Name to check for availability.</param>
    /// <param name="excludeId">Optional pharmaceutical form id to exclude.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns availability information.</response>
    /// <response code="400">Invalid query parameters.</response>
    [HttpGet("name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(PharmaceuticalFormNameAvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PharmaceuticalFormNameAvailabilityDto>> Check(
        [FromQuery] string name,
        [FromQuery(Name = "exclude_id")] Guid? excludeId,
        CancellationToken cancellationToken)
    {
        CheckPharmaceuticalFormNameAvailabilityRequest request = new(name, excludeId);

        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        CheckPharmaceuticalFormNameAvailabilityQuery query = request.MapToQuery();
        ErrorOr<PharmaceuticalFormNameAvailabilityDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}


