using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.GetImportBatchDetails;

internal sealed class GetImportBatchDetailsHandler(
    ITempDrugImportBatchRepository _batchRepository)
    : IRequestHandler<GetImportBatchDetailsQuery, ErrorOr<ImportBatchDetailsDto>>
{
    public async Task<ErrorOr<ImportBatchDetailsDto>> Handle(
        GetImportBatchDetailsQuery query,
        CancellationToken cancellationToken)
    {
        TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(query.Id, cancellationToken);
        if (batch is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found.",
                metadata: new()
                {
                    ["resourceType"] = "ImportBatch",
                    ["resourceId"] = query.Id.ToString()
                });
        }

        return batch.MapToDetailsDto();
    }
}
