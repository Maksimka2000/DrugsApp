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

namespace DrugsApp.Features.AllergiesFeature.ListDrugAllergies;

/// <summary>
/// Request model for listing drug allergies.
/// </summary>
public sealed record ListDrugAllergiesRequest(int Skip, int Take);

/// <summary>
/// Query for listing drug allergies.
/// </summary>
public sealed record ListDrugAllergiesQuery(
    Guid UserId,
    int Skip,
    int Take) : IRequest<ErrorOr<ListDrugAllergiesResponse>>;

/// <summary>
/// Controller for listing drug allergies.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class ListDrugAllergiesController(
    IMediator _mediator,
    IValidator<ListDrugAllergiesRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for listing drug allergies.
    /// </summary>
    public const string RouteName = "ListDrugAllergies";

    /// <summary>
    /// Lists drug allergies for the current user.
    /// </summary>
    [HttpGet("drugs", Name = RouteName)]
    [ProducesResponseType(typeof(ListDrugAllergiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListDrugAllergiesResponse>> List(
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        ListDrugAllergiesRequest request = new(skip, take);

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

        ListDrugAllergiesQuery query = request.MapToQuery(userId);
        ErrorOr<ListDrugAllergiesResponse> result = await _mediator.Send(query, cancellationToken);

        return result.Match<ActionResult<ListDrugAllergiesResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
