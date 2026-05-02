namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Details view exposing ID lists for an import batch.
/// </summary>
public sealed record ImportBatchDetailsDto(
    Guid Id,
    List<string> ImportedIds,
    List<string> SkippedIds,
    List<string> FailedToCreate,
    List<string> RecordsDeletedInRevertIds,
    List<string> FailedToDelete);
