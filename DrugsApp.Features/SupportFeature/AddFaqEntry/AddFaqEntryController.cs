using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SupportFeature.Shared;
using DrugsApp.Features.SupportFeature.UpdateFaqEntry;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupportFeature.AddFaqEntry;

/// <summary>
/// Request model for creating a FAQ entry with English and Polish translations.
/// </summary>
public sealed record AddFaqEntryRequest(
    FaqTranslationContentDto En,
    FaqTranslationContentDto Pl);

/// <summary>
/// Response returned after a FAQ entry is created.
/// </summary>
public sealed record CreatedFaqEntryResponse(
    AdminFaqEntryDto FaqEntry);

/// <summary>
/// Command for creating a FAQ entry.
/// </summary>
public sealed record AddFaqEntryCommand(
    AddFaqEntryRequest Request) : IRequest<ErrorOr<CreatedFaqEntryResponse>>;

/// <summary>
/// Controller for creating FAQ entries on the admin support surface.
/// </summary>
[ApiController]
[Tags("Support")]
[Route("api/support/admin/faqs")]
public sealed class AddFaqEntryController(
    IMediator mediator,
    IValidator<AddFaqEntryRequest> validator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IValidator<AddFaqEntryRequest> _validator = validator;

    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "AddFaqEntry";

    /// <summary>
    /// Creates a new FAQ entry.
    /// </summary>
    /// <param name="request">Create request containing English and Polish FAQ content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns the created FAQ entry.</response>
    /// <response code="400">Invalid request payload.</response>
    /// <response code="409">A FAQ entry with the same normalized question already exists.</response>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreatedFaqEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedFaqEntryResponse>> Create(
        [FromBody] AddFaqEntryRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddFaqEntryCommand command = request.MapToCommand();
        ErrorOr<CreatedFaqEntryResponse> response = await _mediator.Send(command, cancellationToken);

        return response.Match(
            created => CreatedAtRoute(
                routeName: UpdateFaqEntryController.RouteName,
                routeValues: new { id = created.FaqEntry.Id },
                value: created),
            errors => errors.MapToProblem(HttpContext));
    }
}
