using System;
using DrugsApp.Domain.TempDrugFeature.Entities;

namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Mapping helpers for import batch aggregate to API DTOs.
/// </summary>
internal static class ImportBatchMappings
{
    public static ImportBatchDto MapToDto(
        this TempDrugImportBatch batch,
        string? hangfireJobStatus = null) => new(
        Id: batch.Id,
        HangfireJobId: batch.HangfireJobId,
        HangfireJobStatus: hangfireJobStatus ?? batch.HangfireJobStatus,
        ImportedBy: batch.ImportedBy,
        NameOfFile: batch.NameOfFile,
        BlobFileId: batch.BlobFileId,
        ImportStartedAt: batch.ImportStartedAt,
        ImportFinishedAt: batch.ImportFinishedAt,
        SourceType: batch.SourceType,
        SourcePath: batch.SourcePath,
        MappingPlanJson: batch.MappingPlanJson,
        TotalQuantityCount: batch.TotalQuantityCount,
        ImportedCount: batch.ImportedCount,
        SkippedCounter: batch.SkippedCounter,
        FailedToCreateCounter: batch.FailedToCreate.Count,
        RecordsDeletedInRevertCounter: batch.RecordsDeletedInRevertCounter,
        FailedToDeleteCounter: batch.FailedToDelete.Count,
        Notes: batch.Notes,
        ErrorNote: batch.ErrorNote,
        StatusOfImport: batch.StatusOfImport,
        ScheduledInfo: batch.ScheduledInfo,
        CreatedAtUtc: batch.CreatedAtUtc,
        UpdatedAtUtc: batch.UpdatedAtUtc);

    public static List<ImportBatchDto> MapToDtoList(
        this IEnumerable<TempDrugImportBatch> batches,
        Func<TempDrugImportBatch, string?>? hangfireJobStatusFactory = null)
        => batches
            .Select(batch => batch.MapToDto(hangfireJobStatusFactory?.Invoke(batch)))
            .ToList();

    public static CreatedImportBatchResponse MapToCreatedResponse(
        this TempDrugImportBatch batch,
        List<string> headers,
        Dictionary<string, string> defaultMapping) => new(
        BatchId: batch.Id,
        TotalQuantityCount: batch.TotalQuantityCount,
        Headers: headers,
        TempDrugFields: TempDrugFields.All.ToList(),
        DefaultMapping: defaultMapping);

    public static ImportBatchDetailsDto MapToDetailsDto(this TempDrugImportBatch batch) => new(
        Id: batch.Id,
        ImportedIds: batch.ImportedIds.ToList(),
        SkippedIds: batch.SkippedIds.ToList(),
        FailedToCreate: batch.FailedToCreate.ToList(),
        RecordsDeletedInRevertIds: batch.RecordsDeletedInRevertIds.ToList(),
        FailedToDelete: batch.FailedToDelete.ToList());
}

