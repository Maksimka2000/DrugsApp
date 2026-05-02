using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SupplementFeature.Shared;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SupplementFeature.AddSupplement;

/// <summary>
/// Request model for creating a supplement.
/// </summary>
public sealed record AddSupplementRequest(
    string Name,
    IReadOnlyCollection<string>? Aliases);

/// <summary>
/// Command for adding a supplement.
/// </summary>
public sealed record AddSupplementCommand(
    AddSupplementRequest Request) : IRequest<ErrorOr<CreatedSupplementResponse>>;

/// <summary>
/// Response returned after creating a supplement.
/// </summary>
public sealed record CreatedSupplementResponse(
    SupplementDto Supplement);



/// <summary>
/// Controller for creating supplements.
/// </summary>
[ApiController]
[Tags("Supplements")]
[Route("api/supplements")]
public sealed class AddSupplementController(
    IMediator _mediator,
    IValidator<AddSupplementRequest> _validator) : ControllerBase
{
    /// <summary>
    /// Route name used for CreatedAtRoute.
    /// </summary>
    public const string RouteName = "AddSupplement";

    /// <summary>
    /// Creates a new supplement.
    /// </summary>
    /// <param name="request">Create supplement payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Returns created supplement.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="409">Supplement with the same name already exists.</response>
    [HttpPost(Name = RouteName)]
    [ProducesResponseType(typeof(CreatedSupplementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreatedSupplementResponse>> Create(
        [FromBody] AddSupplementRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        AddSupplementCommand command = request.MapToCommand();
        ErrorOr<CreatedSupplementResponse> result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            response => CreatedAtRoute(
                routeName: UpdateSupplement.UpdateSupplementController.RouteName,
                routeValues: new { id = response.Supplement.Id },
                value: response),
            errors => errors.MapToProblem(HttpContext));
    }
}
