using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.SystemTextFeature.Shared;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.SystemTextFeature.AlcoholInteractionDefaultText;



/// <summary>
/// Query for retrieving the default alcohol interaction text.
/// </summary>
public sealed record GetAlcoholInteractionDefaultTextQuery
    : IRequest<ErrorOr<AlcoholInteractionDefaultTextDto>>;

/// <summary>
/// Request payload for updating the default alcohol interaction text.
/// </summary>
public sealed record UpdateAlcoholInteractionDefaultTextRequest(string? DefaultText);

/// <summary>
/// Command for updating the default alcohol interaction text.
/// </summary>
public sealed record UpdateAlcoholInteractionDefaultTextCommand(string DefaultText)
    : IRequest<ErrorOr<AlcoholInteractionDefaultTextDto>>;




/// <summary>
/// Controller for managing the default alcohol interaction text.
/// </summary>
[ApiController]
[Tags("Alcohol Interactions")]
[Route("api/alcohol-interactions/default-text")]
public sealed class AlcoholInteractionDefaultTextController(
    IMediator _mediator,
    IValidator<UpdateAlcoholInteractionDefaultTextRequest> _validator) : ControllerBase
{
    public const string GetRouteName = "GetAlcoholInteractionDefaultText";
    public const string PatchRouteName = "UpdateAlcoholInteractionDefaultText";

    /// <summary>
    /// Retrieves the default alcohol interaction text, creating it if missing.
    /// </summary>
    [HttpGet(Name = GetRouteName)]
    [ProducesResponseType(typeof(AlcoholInteractionDefaultTextDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AlcoholInteractionDefaultTextDto>> Get(CancellationToken cancellationToken)
    {
        ErrorOr<AlcoholInteractionDefaultTextDto> result = await _mediator.Send(
            new GetAlcoholInteractionDefaultTextQuery(),
            cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }

    /// <summary>
    /// Updates the default alcohol interaction text.
    /// </summary>
    [HttpPatch(Name = PatchRouteName)]
    [ProducesResponseType(typeof(AlcoholInteractionDefaultTextDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlcoholInteractionDefaultTextDto>> Update(
        [FromBody] UpdateAlcoholInteractionDefaultTextRequest request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.MapToProblem(HttpContext);
        }

        UpdateAlcoholInteractionDefaultTextCommand command = request.MapToCommand();
        ErrorOr<AlcoholInteractionDefaultTextDto> result = await _mediator.Send(
            command,
            cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}
