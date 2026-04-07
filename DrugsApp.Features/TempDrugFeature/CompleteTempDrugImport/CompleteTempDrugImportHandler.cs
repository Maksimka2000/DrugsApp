using DrugsApp.Domain.TempDrugFeature.Entities;
using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.CompleteTempDrugImport;

/// <summary>
/// Internal command for finalizing successful import.
/// </summary>
public sealed record CompleteTempDrugImportCommand(Guid BatchId) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Handles final import completion transition.
/// </summary>
internal sealed class CompleteTempDrugImportHandler(ITempDrugImportBatchRepository _batchRepository)
    : IRequestHandler<CompleteTempDrugImportCommand, ErrorOr<Success>>
{

    public async Task<ErrorOr<Success>> Handle(CompleteTempDrugImportCommand command, CancellationToken cancellationToken)
    {
        TempDrugImportBatch? batch = await _batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
        if (batch is null)
        {
            return Error.NotFound(
                description: "The requested resource was not found.",
                metadata: new()
                {
                    ["resourceType"] = "ImportBatch",
                    ["resourceId"] = command.BatchId.ToString()
                });
        }

        ErrorOr<Success> completeResult = batch.CompleteImport();
        if (completeResult.IsError)
        {
            return completeResult.Errors;
        }

        return Result.Success;
    }
}

