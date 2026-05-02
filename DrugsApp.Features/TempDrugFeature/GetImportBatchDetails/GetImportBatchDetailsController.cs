using DrugsApp.Features.Common.Extensions;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DrugsApp.Features.TempDrugFeature.GetImportBatchDetails;

public sealed record GetImportBatchDetailsQuery(Guid Id) : IRequest<ErrorOr<ImportBatchDetailsDto>>;

[ApiController]
[Tags("TempDrugImport")]
[Route("api/temp-drug-imports")]
public sealed class GetImportBatchDetailsController(IMediator _mediator) : ControllerBase
{
    [HttpGet("{id:guid}/details")]
    public async Task<ActionResult<ImportBatchDetailsDto>> GetDetails(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        GetImportBatchDetailsQuery query = new(id);
        ErrorOr<ImportBatchDetailsDto> response = await _mediator.Send(query, cancellationToken);

        return response.Match(
            result => Ok(result),
            errors => errors.MapToProblem(HttpContext));
    }
}
