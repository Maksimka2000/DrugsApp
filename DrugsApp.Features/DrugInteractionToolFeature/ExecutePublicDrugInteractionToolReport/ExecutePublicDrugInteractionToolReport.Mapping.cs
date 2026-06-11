namespace DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;

/// <summary>
/// Mapping helpers for the public drug interaction tool report use case [description].
/// </summary>
public static class ExecutePublicDrugInteractionToolReportMapping
{
  public static ExecutePublicDrugInteractionToolReportCommand MapToCommand(
    this ExecutePublicDrugInteractionToolReportRequest request) =>
    new(Request: request);
}
