using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.GetImportBatch;

/// <summary>
/// Query model for import batch details retrieval.
/// </summary>
public sealed record GetImportBatchQuery(Guid Id) : IRequest<ErrorOr<ImportBatchDto>>;

/// <summary>
/// Controller for loading single import batch details.
/// </summary>
[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-imports")]
public sealed class GetImportBatchController(IMediator _mediator) : ControllerBase
{
    
    /// <summary>
    /// Returns full details of import batch by id.
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetImportBatch")]
    public async Task<ActionResult<ImportBatchDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        GetImportBatchQuery query = GetImportBatchMapping.MapToQuery(id);
        ErrorOr<ImportBatchDto> response = await _mediator.Send(query, cancellationToken);

        return response.Match(
            response => Ok(response),
            errors => errors.MapToProblem(HttpContext));
    }
}

