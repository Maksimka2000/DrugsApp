using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.DrugFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.DrugFeature.RetrieveDrug;

public sealed record RetrieveDrugQuery(Guid DrugId) : IRequest<ErrorOr<DrugDto>>;


    /// <summary>
    /// Controller for retrieving a drug by id. <remarks>
    /// Initializes a new instance of the <see cref="RetrieveDrugController"/> class.
    /// </remarks>
    /// </summary>
    /// <param name="_mediator">MediatR mediator.</param>
[ApiController]
[Tags("Drugs")]
[Route("api/drugs")]
public sealed class RetrieveDrugController(IMediator _mediator) : ControllerBase
{

/// <summary>
/// Retrieves a drug by id. 
/// </summary>
/// <param name="id">Drug id.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The requested drug.</returns>
/// <response code="200">Returns the requested drug.</response>
/// <response code="404">Drug not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DrugDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DrugDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        RetrieveDrugQuery query = RetrieveDrugMapping.MapToQuery(id);
        ErrorOr<DrugDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

