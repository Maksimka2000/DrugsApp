using DrugsApp.Domain.Common;
using ErrorOr;

namespace DrugsApp.Domain.TempDrugFeature.Entities;

/// <summary>
/// Represents an import batch that tracks upload, import, and revert execution state.
/// </summary>
public sealed class TempDrugImportBatch : BaseEntity
{
    private TempDrugImportBatch() : base()
    {
        ImportedIds = [];
        SkippedIds = [];
        FailedToCreate = [];
        RecordsDeletedInRevertIds = [];
        FailedToDelete = [];
    }

    /// <summary>
    /// Current Hangfire job identifier for import or revert operation.
    /// </summary>
    public string? HangfireJobId { get; private set; }

    /// <summary>
    /// Current Hangfire job status for import or revert operation.
    /// </summary>
    public string? HangfireJobStatus { get; private set; }

    /// <summary>
    /// Identifier of user who initiated import (null until auth is implemented).
    /// </summary>
    public string? ImportedBy { get; private set; }

    /// <summary>
    /// Original uploaded file name.
    /// </summary>
    public string NameOfFile { get; private set; } = null!;

    /// <summary>
    /// Blob identifier of the uploaded file in storage.
    /// </summary>
    public Guid BlobFileId { get; private set; }

    /// <summary>
    /// UTC timestamp when import execution started.
    /// </summary>
    public DateTime? ImportStartedAt { get; private set; }

    /// <summary>
    /// UTC timestamp when import or revert execution finished.
    /// </summary>
    public DateTime? ImportFinishedAt { get; private set; }

    /// <summary>
    /// Source descriptor (for example file or URL hostname).
    /// </summary>
    public string SourceType { get; private set; } = null!;

    /// <summary>
    /// Optional source path such as URL for URL import.
    /// </summary>
    public string? SourcePath { get; private set; }

    /// <summary>
    /// JSON payload of selected mapping plan sent before import start.
    /// </summary>
    public string? MappingPlanJson { get; private set; }

    /// <summary>
    /// Total number of source rows discovered in uploaded file.
    /// </summary>
    public int TotalQuantityCount { get; private set; }

    /// <summary>
    /// Number of rows successfully imported into TempDrugs.
    /// </summary>
    public int ImportedCount { get; private set; }

    /// <summary>
    /// Product identifiers imported successfully.
    /// </summary>
    public List<string> ImportedIds { get; private set; }

    /// <summary>
    /// Number of rows skipped due to duplication.
    /// </summary>
    public int SkippedCounter { get; private set; }

    /// <summary>
    /// Number of rows that failed creation.
    /// </summary>
    public int FailedToCreateCounter { get; private set; }

    /// <summary>
    /// Product identifiers skipped due to duplication.
    /// </summary>
    public List<string> SkippedIds { get; private set; }

    /// <summary>
    /// Product identifiers that failed to create as TempDrugs.
    /// </summary>
    public List<string> FailedToCreate { get; private set; }

    /// <summary>
    /// Number of TempDrugs deleted during revert.
    /// </summary>
    public int RecordsDeletedInRevertCounter { get; private set; }

    /// <summary>
    /// TempDrug ids deleted during revert.
    /// </summary>
    public List<string> RecordsDeletedInRevertIds { get; private set; }

    /// <summary>
    /// Number of TempDrugs that failed to delete during revert.
    /// </summary>
    public int FailedToDeleteCounter { get; private set; }

    /// <summary>
    /// TempDrug product identifiers that could not be deleted during revert.
    /// </summary>
    public List<string> FailedToDelete { get; private set; }

    /// <summary>
    /// Optional administrator note.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Current batch status.
    /// </summary>
    public ImportBatchStatus StatusOfImport { get; private set; }

    /// <summary>
    /// Optional error note captured when a failure occurs.
    /// </summary>
    public string? ErrorNote { get; private set; }

    /// <summary>
    /// Optional UTC schedule for delayed execution.
    /// </summary>
    public DateTime? ScheduledInfo { get; private set; }

    /// <summary>
    /// Creates a new import batch after upload and file analysis.
    /// </summary>
    public static ErrorOr<TempDrugImportBatch> Create(
        string nameOfFile,
        Guid blobFileId,
        string sourceType,
        string? sourcePath,
        int totalQuantityCount)
    {
        if (string.IsNullOrWhiteSpace(nameOfFile))
        {
            return Error.Validation(description: "NameOfFile is required.");
        }

        if (blobFileId == Guid.Empty)
        {
            return Error.Validation(
                description: "BlobFileId must not be empty.",
                metadata: new()
                {
                    ["error"] = "blobFileId"
                });
        }

        if (string.IsNullOrWhiteSpace(sourceType))
        {
            return Error.Validation(description: "SourceType is required.");
        }

        if (totalQuantityCount < 0)
        {
            return Error.Validation(description: "TotalQuantityCount must be greater than or equal to 0.");
        }

        return new TempDrugImportBatch
        {
            NameOfFile = nameOfFile.Trim(),
            BlobFileId = blobFileId,
            SourceType = sourceType.Trim(),
            SourcePath = NormalizeNullable(sourcePath),
            TotalQuantityCount = totalQuantityCount,
            ImportedCount = 0,
            SkippedCounter = 0,
            RecordsDeletedInRevertCounter = 0,
            StatusOfImport = ImportBatchStatus.NotStarted
        };
    }

    /// <summary>
    /// Starts import processing and transitions status to in-progress.
    /// </summary>
    public ErrorOr<Success> StartJob(
        string mappingPlanJson,
        string? notes)
    {
        if (StatusOfImport != ImportBatchStatus.NotStarted)
        {
            return Error.Conflict(
                description: "Import can be started only when status is not_started.",
                metadata: new()
                {
                    ["resourceId"] = Id.ToString(),
                    ["currentStatus"] = StatusOfImport.ToString()
                });
        }
        
        if (string.IsNullOrWhiteSpace(mappingPlanJson))
        {
            return Error.Validation(description: "MappingPlanJson is required.");
        }
        
        MappingPlanJson = mappingPlanJson;
        Notes = NormalizeNullable(notes);
        ImportStartedAt = DateTime.UtcNow;
        ImportFinishedAt = null;
        StatusOfImport = ImportBatchStatus.InProgress;
        return Result.Success;
    }

    /// <summary>
    /// Accumulates import counters for a single processed chunk.
    /// </summary>
    public void RecordChunkImported(
        int importedCount,
        IReadOnlyCollection<string>? importedIds,
        int skippedCount,
        IReadOnlyCollection<string>? skippedIds,
        IReadOnlyCollection<string>? failedIds)
    {
        if (importedCount > 0)
        {
            ImportedCount += importedCount;
        }

        if (skippedCount > 0)
        {
            SkippedCounter += skippedCount;
        }

        if (importedIds is not null && importedIds.Count > 0)
        {
            ImportedIds.AddRange(importedIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()));
        }

        if (skippedIds is not null && skippedIds.Count > 0)
        {
            SkippedIds.AddRange(skippedIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()));
        }

        if (failedIds is not null && failedIds.Count > 0)
        {
            FailedToCreate.AddRange(failedIds.Where(id => !string.IsNullOrWhiteSpace(id)).Select(id => id.Trim()));
            FailedToCreateCounter += failedIds.Count(id => !string.IsNullOrWhiteSpace(id));
        }
    }

    /// <summary>
    /// Completes import processing successfully.
    /// </summary>
    public ErrorOr<Success> CompleteImport()
    {
        if (StatusOfImport != ImportBatchStatus.InProgress)
        {
            return Error.Conflict(
                description: "Import can be completed only from in_progress state.",
                metadata: new()
                {
                    ["resourceId"] = Id.ToString(),
                    ["currentStatus"] = StatusOfImport.ToString()
                });
        }

        ImportFinishedAt = DateTime.UtcNow;
        StatusOfImport = ImportBatchStatus.ImportedSuccessfully;
        return Result.Success;
    }

    /// <summary>
    /// Marks import processing as failed.
    /// </summary>
    public void MarkError(string? errorNote = null)
    {
        ImportFinishedAt = DateTime.UtcNow;
        StatusOfImport = ImportBatchStatus.Error;
        ErrorNote = NormalizeNullable(errorNote);
    }

    /// <summary>
    /// Cancels currently running import or revert processing.
    /// </summary>
    public ErrorOr<Success> Cancel()
    {
        if (StatusOfImport is not ImportBatchStatus.InProgress and not ImportBatchStatus.RevertInProgress)
        {
            return Error.Conflict(
                description: "Batch can be cancelled only when import or revert is in progress.",
                metadata: new()
                {
                    ["resourceId"] = Id.ToString(),
                    ["currentStatus"] = StatusOfImport.ToString()
                });
        }

        ImportFinishedAt = DateTime.UtcNow;
        StatusOfImport = ImportBatchStatus.Cancelled;
        return Result.Success;
    }

    /// <summary>
    /// Starts revert processing and transitions status to revert in progress.
    /// </summary>
    public ErrorOr<Success> StartRevert()
    {
        if (StatusOfImport is not ImportBatchStatus.ImportedSuccessfully
            and not ImportBatchStatus.Error
            and not ImportBatchStatus.Cancelled)
        {
            return Error.Conflict(
                description: "Revert can be started only from imported_successfully, error, or cancelled state.",
                metadata: new()
                {
                    ["resourceId"] = Id.ToString(),
                    ["currentStatus"] = StatusOfImport.ToString()
                });
        }

        ImportStartedAt = DateTime.UtcNow;
        // ImportFinishedAt = null;
        StatusOfImport = ImportBatchStatus.RevertInProgress;
        return Result.Success;
    }

    /// <summary>
    /// Sets job identifier for currently running revert job.
    /// </summary>
    public void SetJobId(string jobId)
    {
        HangfireJobId = string.IsNullOrWhiteSpace(jobId) ? null : jobId.Trim();
        HangfireJobStatus = null;
    }

    /// <summary>
    /// Updates persisted Hangfire job status snapshot.
    /// </summary>
    public void SetJobStatus(string? status)
    {
        HangfireJobStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
    }

    /// <summary>
    /// Accumulates revert counters for a single processed chunk.
    /// </summary>
    public void RecordChunkReverted(
        int deletedCount,
        IReadOnlyCollection<string>? deletedProductIdentifiers,
        IReadOnlyCollection<string>? failedDeleteProductIdentifiers)
    {
        if (deletedCount > 0)
        {
            RecordsDeletedInRevertCounter += deletedCount;
        }

        if (deletedProductIdentifiers is not null && deletedProductIdentifiers.Count > 0)
        {
            RecordsDeletedInRevertIds.AddRange(
                deletedProductIdentifiers
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim()));
        }

        if (failedDeleteProductIdentifiers is not null && failedDeleteProductIdentifiers.Count > 0)
        {
            FailedToDelete.AddRange(
                failedDeleteProductIdentifiers
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim()));
            FailedToDeleteCounter += failedDeleteProductIdentifiers.Count(id => !string.IsNullOrWhiteSpace(id));
        }
    }

    /// <summary>
    /// Completes revert processing successfully.
    /// </summary>
    public void CompleteRevert()
    {
        ImportFinishedAt = DateTime.UtcNow;
        StatusOfImport = ImportBatchStatus.RevertedSuccessfully;
    }

    /// <summary>
    /// Marks revert processing as failed.
    /// </summary>
    public void MarkRevertError(string? errorNote = null)
    {
        ImportFinishedAt = DateTime.UtcNow;
        StatusOfImport = ImportBatchStatus.Error;
        ErrorNote = NormalizeNullable(errorNote);
    }

    private static string? NormalizeNullable(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

