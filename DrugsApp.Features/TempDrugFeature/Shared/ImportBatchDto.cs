using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Full snapshot of temporary drug import batch metadata.
/// </summary>
public sealed record ImportBatchDto(
    Guid Id,
    string? HangfireJobId,
    string? HangfireJobStatus,
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
    int SkippedCounter,
    int FailedToCreateCounter,
    int RecordsDeletedInRevertCounter,
    int FailedToDeleteCounter,
    string? Notes,
    string? ErrorNote,
    ImportBatchStatus StatusOfImport,
    DateTime? ScheduledInfo,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

