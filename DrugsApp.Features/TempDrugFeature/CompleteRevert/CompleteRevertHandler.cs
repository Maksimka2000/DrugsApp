using DrugsApp.Domain.TempDrugFeature.Repositories;
using ErrorOr;
using MediatR;

namespace DrugsApp.Features.TempDrugFeature.CompleteRevert;

/// <summary>
/// Internal command for finalizing revert execution.
/// </summary>
public sealed record CompleteRevertCommand(Guid BatchId) : IRequest<ErrorOr<Success>>;

/// <summary>
/// Handles final revert completion transition.
/// </summary>
internal sealed class CompleteRevertHandler(ITempDrugImportBatchRepository _batchRepository)
    : IRequestHandler<CompleteRevertCommand, ErrorOr<Success>>
{
    public async Task<ErrorOr<Success>> Handle(CompleteRevertCommand command, CancellationToken cancellationToken)
    {
        var batch = await _batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
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

        batch.CompleteRevert();
        return Result.Success;
    }
}

