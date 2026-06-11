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

namespace DrugsApp.Features.AllergiesFeature.ListOtherAllergies;

public sealed record ListOtherAllergiesRequest(int Skip, int Take);
public sealed record ListOtherAllergiesQuery(Guid UserId, int Skip, int Take) : IRequest<ErrorOr<ListOtherAllergiesResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class ListOtherAllergiesController(
    IMediator _mediator,
    IValidator<ListOtherAllergiesRequest> _validator) : ControllerBase
{
    public const string RouteName = "ListOtherAllergies";

    [HttpGet("other-allergies", Name = RouteName)]
    [ProducesResponseType(typeof(ListOtherAllergiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListOtherAllergiesResponse>> List(
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        ListOtherAllergiesRequest request = new(skip, take);
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid) return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId)) return Unauthorized();

        ListOtherAllergiesQuery query = request.MapToQuery(userId);
        ErrorOr<ListOtherAllergiesResponse> result = await _mediator.Send(query, cancellationToken);
        return result.Match<ActionResult<ListOtherAllergiesResponse>>(response => Ok(response), errors => errors.MapToProblem(HttpContext));
    }
}
