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

namespace DrugsApp.Features.AllergiesFeature.CheckOtherAllergyNameAvailability;

public sealed record CheckOtherAllergyNameAvailabilityRequest(string AllergyName, Guid? ExcludeId);
public sealed record CheckOtherAllergyNameAvailabilityQuery(Guid UserId, string AllergyName, Guid? ExcludeId)
    : IRequest<ErrorOr<NameAvailabilityResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class CheckOtherAllergyNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckOtherAllergyNameAvailabilityRequest> _validator) : ControllerBase
{
    public const string RouteName = "CheckOtherAllergyNameAvailability";

    [HttpGet("other-allergies/name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(NameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NameAvailabilityResponse>> Check(
        [FromQuery(Name = "name")] string allergyName,
        [FromQuery(Name = "exclude_id")] Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        CheckOtherAllergyNameAvailabilityRequest request = new(allergyName, excludeId);
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId)) return Unauthorized();

        CheckOtherAllergyNameAvailabilityQuery query = request.MapToQuery(userId);
        ErrorOr<NameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);
        return result.Match<ActionResult<NameAvailabilityResponse>>(response => Ok(response), errors => errors.MapToProblem(HttpContext));
    }
}
