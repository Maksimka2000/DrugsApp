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

namespace DrugsApp.Features.AllergiesFeature.CheckSupplementCustomNameAvailability;

public sealed record CheckSupplementCustomNameAvailabilityRequest(string CustomSupplementName);
public sealed record CheckSupplementCustomNameAvailabilityQuery(Guid UserId, string CustomSupplementName)
    : IRequest<ErrorOr<NameAvailabilityResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class CheckSupplementCustomNameAvailabilityController(
    IMediator _mediator,
    IValidator<CheckSupplementCustomNameAvailabilityRequest> _validator) : ControllerBase
{
    public const string RouteName = "CheckSupplementCustomNameAvailability";

    [HttpGet("supplements/custom-name-availability", Name = RouteName)]
    [ProducesResponseType(typeof(NameAvailabilityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<NameAvailabilityResponse>> Check(
        [FromQuery(Name = "custom_supplement_name")] string customSupplementName,
        CancellationToken cancellationToken = default)
    {
        CheckSupplementCustomNameAvailabilityRequest request = new(customSupplementName);
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId)) return Unauthorized();

        CheckSupplementCustomNameAvailabilityQuery query = request.MapToQuery(userId);
        ErrorOr<NameAvailabilityResponse> result = await _mediator.Send(query, cancellationToken);
        return result.Match<ActionResult<NameAvailabilityResponse>>(response => Ok(response), errors => errors.MapToProblem(HttpContext));
    }
}
