using System;
using System.Threading;
using System.Threading.Tasks;
using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.VetDrugFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.VetDrugFeature.RetrieveVetDrug;

public sealed record RetrieveVetDrugQuery(Guid VetDrugId) : IRequest<ErrorOr<VetDrugDto>>;

/// <summary>
/// Controller for retrieving a veterinary drug by id.
/// </summary>
/// <param name="_mediator">The mediator.</param>
[ApiController]
[Tags("Vet Drugs")]
[Route("api/vet-drugs")]
public sealed class RetrieveVetDrugController(IMediator _mediator) : ControllerBase
{
    /// <summary>
    /// Route name used for link generation.
    /// </summary>
    public const string RouteName = "RetrieveVetDrug";

    /// <summary>
    /// Retrieves a veterinary drug by id.
    /// </summary>
    /// <param name="id">The id of the veterinary drug.</param>
    /// <param name="cancellationToken"></param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VetDrugDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VetDrugDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {

        RetrieveVetDrugQuery query = RetrieveVetDrugMapping.MapToQuery(id);
        ErrorOr<VetDrugDto> result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

