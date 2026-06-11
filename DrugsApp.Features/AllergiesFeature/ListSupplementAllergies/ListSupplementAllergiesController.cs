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

namespace DrugsApp.Features.AllergiesFeature.ListSupplementAllergies;

public sealed record ListSupplementAllergiesRequest(int Skip, int Take);

public sealed record ListSupplementAllergiesQuery(
    Guid UserId,
    int Skip,
    int Take) : IRequest<ErrorOr<ListSupplementAllergiesResponse>>;

[ApiController]
[Authorize(Roles = AuthRoles.MemberRoles)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class ListSupplementAllergiesController(
    IMediator _mediator,
    IValidator<ListSupplementAllergiesRequest> _validator) : ControllerBase
{
    public const string RouteName = "ListSupplementAllergies";

    [HttpGet("supplements", Name = RouteName)]
    [ProducesResponseType(typeof(ListSupplementAllergiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListSupplementAllergiesResponse>> List(
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "take")] int take = 20,
        CancellationToken cancellationToken = default)
    {
        ListSupplementAllergiesRequest request = new(skip, take);
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.MapToProblem(HttpContext);

        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
            return Unauthorized();

        ListSupplementAllergiesQuery query = request.MapToQuery(userId);
        ErrorOr<ListSupplementAllergiesResponse> result = await _mediator.Send(query, cancellationToken);
        return result.Match<ActionResult<ListSupplementAllergiesResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
