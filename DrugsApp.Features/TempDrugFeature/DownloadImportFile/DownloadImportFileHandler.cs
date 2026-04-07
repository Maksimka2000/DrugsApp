using DrugsApp.Domain.Common;
using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.DownloadImportFile;

/// <summary>
/// Handles import file download query.
/// </summary>
internal sealed class DownloadImportFileHandler(
    ITempDrugImportBatchRepository _batchRepository,
    IBlobService _blobService)
    : IRequestHandler<DownloadImportFileQuery, ErrorOr<DownloadImportFileResponse>>
{
    public async Task<ErrorOr<DownloadImportFileResponse>> Handle(
        DownloadImportFileQuery query,
        CancellationToken cancellationToken)
    {
        TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(query.BatchId, cancellationToken);
        if (batch is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found.",
                metadata: new()
                {
                    ["resourceType"] = "ImportBatch",
                    ["resourceId"] = query.BatchId.ToString()
                });
        }

        try
        {
            FileResponse file = await _blobService.DownloadAsync(batch.BlobFileId, cancellationToken);
            return new DownloadImportFileResponse(file.Stream, file.ContentType, batch.NameOfFile);
        }
        catch (FileNotFoundException)
        {
            return Error.NotFound(
                description: "The requested file was not found.",
                metadata: new()
                {
                    ["resourceType"] = "ImportFile",
                    ["resourceId"] = batch.BlobFileId.ToString()
                });
        }
    }
}

