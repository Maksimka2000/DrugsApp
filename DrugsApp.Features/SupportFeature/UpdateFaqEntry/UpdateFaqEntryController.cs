using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SupportFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupportFeature.UpdateFaqEntry;

/// <summary>
/// Request model for partially updating a FAQ entry.
/// </summary>
public sealed record UpdateFaqEntryRequest(
    string? Question,
    string? Answer);

/// <summary>
/// Response returned after a FAQ entry update.
/// </summary>
public sealed record UpdatedFaqEntryResponse(
    AdminFaqEntryDto FaqEntry);

/// <summary>
/// Command for partially updating a FAQ entry.
/// </summary>
public sealed record UpdateFaqEntryCommand(
    Guid Id,
    UpdateFaqEntryRequest Request) : IRequest<ErrorOr<UpdatedFaqEntryResponse>>;

/// <summary>
/// Controller for updating FAQ entries on the admin support surface.
/// </summary>
[ApiController]
[Tags("Support")]
[Route("api/support/admin/faqs")]
public sealed class UpdateFaqEntryController(
    IMediator mediator,
    IValidator<UpdateFaqEntryRequest> validator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IValidator<UpdateFaqEntryRequest> _validator = validator;

    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "UpdateFaqEntry";

    /// <summary>
    /// Partially updates an existing FAQ entry.
    /// </summary>
    /// <param name="id">FAQ entry identifier.</param>
    /// <param name="request">PATCH payload with FAQ fields to modify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the updated FAQ entry.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="404">FAQ entry not found.</response>
    /// <response code="409">A FAQ entry with the same normalized question already exists.</response>
    [HttpPatch("{id:guid}", Name = RouteName)]
    [ProducesResponseType(typeof(UpdatedFaqEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UpdatedFaqEntryResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateFaqEntryRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateFaqEntryCommand command = request.MapToCommand(id);
        ErrorOr<UpdatedFaqEntryResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
