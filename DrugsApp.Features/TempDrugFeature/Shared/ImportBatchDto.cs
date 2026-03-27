using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Full snapshot of temporary drug import batch metadata.
/// </summary>
public sealed record ImportBatchDto(
    Guid Id,
    string? HangfireJobId,
    string? ImportedBy,
    string NameOfFile,
    Guid BlobFileId,
    DateTime? ImportStartedAt,
    DateTime? ImportFinishedAt,
    string SourceType,
    string? SourcePath,
    string? MappingPlanJson,
    int TotalQuantityCount,
    int ImportedCount,
    List<string> ImportedIds,
    int SkippedCounter,
    List<string> SkippedIds,
    List<string> FailedToCreate,
    int RecordsDeletedInRevert,
    List<string> RecordsDeletedInRevertIds,
    List<string> FailedToDelete,
    string? Notes,
    string? ErrorNote,
    ImportBatchStatus StatusOfImport,
    DateTime? ScheduledInfo,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

