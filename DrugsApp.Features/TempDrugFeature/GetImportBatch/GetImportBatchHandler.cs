using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Interfaces;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using DrugsApp.Features.TempDrugFeature.Shared;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.GetImportBatch;

/// <summary>
/// Handles import batch details query.
/// </summary>
internal sealed class GetImportBatchHandler(
    ITempDrugImportBatchRepository _batchRepository,
    ITempDrugImportJobStatusProvider _jobStatusProvider)
    : IRequestHandler<GetImportBatchQuery, ErrorOr<ImportBatchDto>>
{
    
    public async Task<ErrorOr<ImportBatchDto>> Handle(GetImportBatchQuery query, CancellationToken cancellationToken)
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

        string? hangfireJobStatus = _jobStatusProvider.GetStatus(batch.HangfireJobId);

        return batch.MapToDto(hangfireJobStatus);
    }
}

