namespace DrugsApp.Features.DrugInteractionToolFeature.ExecuteRegisteredDrugInteractionToolReport;

/// <summary>
/// Mapping helpers for the registered drug interaction tool report use case [description].
/// </summary>
public static class ExecuteRegisteredDrugInteractionToolReportMapping
{
  public static ExecuteRegisteredDrugInteractionToolReportCommand MapToCommand(
    this ExecuteRegisteredDrugInteractionToolReportRequest request,
    Guid userId) =>
    new(UserId: userId, Request: request);
}
