namespace DrugsApp.Features.TempDrugFeature.CompleteRevert;

internal static class CompleteRevertMapping
{
    public static CompleteRevertCommand MapToCommand(Guid batchId) => new(BatchId: batchId);
}

