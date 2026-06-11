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

namespace DrugsApp.Features.AllergiesFeature.ListFoodAllergies;

public sealed record ListFoodAllergiesRequest(int Skip, int Take);

public sealed record ListFoodAllergiesQuery(
    Guid UserId,
    int Skip,
    int Take) : IRequest<ErrorOr<ListFoodAllergiesResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class ListFoodAllergiesController(
    IMediator _mediator,
    IValidator<ListFoodAllergiesRequest> _validator) : ControllerBase
{
    public const string RouteName = "ListFoodAllergies";

    [HttpGet("foods", Name = RouteName)]
    [ProducesResponseType(typeof(ListFoodAllergiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListFoodAllergiesResponse>> List(
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        ListFoodAllergiesRequest request = new(skip, take);
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
            return Unauthorized();

        ListFoodAllergiesQuery query = request.MapToQuery(userId);
        ErrorOr<ListFoodAllergiesResponse> result = await _mediator.Send(query, cancellationToken);
        return result.Match<ActionResult<ListFoodAllergiesResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
