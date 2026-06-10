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

namespace DrugsApp.Features.AllergiesFeature.UpsertAllergySummary;

/// <summary>
/// Request model for upserting allergy summary answers.
/// </summary>
public sealed record UpsertAllergySummaryRequest(
    string? SevereReactionStatus,
    bool? CarriesAllergyMedication,
    string? PollenDustMitesAllergyStatus,
    string? InsectVenomAllergyStatus,
    string? LatexAllergyStatus);

/// <summary>
/// Command for upserting allergy summary answers.
/// </summary>
public sealed record UpsertAllergySummaryCommand(
    Guid UserId,
    UpsertAllergySummaryRequest Request) : IRequest<ErrorOr<AllergySummaryResponse>>;

/// <summary>
/// Controller for upserting allergy summary answers.
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.Client)]
[Tags("Allergies")]
[Route("api/allergies")]
public sealed class UpsertAllergySummaryController(
    IMediator _mediator,
    IValidator<UpsertAllergySummaryRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name for upserting allergy summary.
    /// </summary>
    public const string RouteName = "UpsertAllergySummary";

    /// <summary>
    /// Creates or updates the allergy summary for the current user.
    /// </summary>
    [HttpPut("summary", Name = RouteName)]
    [ProducesResponseType(typeof(AllergySummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AllergySummaryResponse>> Upsert(
        [FromBody] UpsertAllergySummaryRequest request,
        CancellationToken cancellationToken = default)
    {
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

        UpsertAllergySummaryCommand command = request.MapToCommand(userId);
        ErrorOr<AllergySummaryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match<ActionResult<AllergySummaryResponse>>(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
