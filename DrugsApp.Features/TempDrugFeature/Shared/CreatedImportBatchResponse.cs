namespace DrugsApp.Features.TempDrugFeature.Shared;

/// <summary>
/// Response returned after upload analysis and import batch creation.
/// </summary>
public sealed record CreatedImportBatchResponse(
    Guid BatchId,
    int TotalQuantityCount,
    List<string> Headers,
    List<string> TempDrugFields,
    Dictionary<string, string> DefaultMapping);

