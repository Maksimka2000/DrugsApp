namespace DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;

/// <summary>
/// Mapping helpers for the public drug interaction tool report use case [description].
/// </summary>
public static class ExecutePublicDrugInteractionToolReportMapping
{
  /// <summary>
  /// Builds the query for executing a public drug interaction tool report.
  /// </summary>
  /// <param name="request">Incoming request payload with manual selections and flags.</param>
  /// <returns>Query carrying the request.</returns>
  public static ExecutePublicDrugInteractionToolReportQuery MapToQuery(
    this ExecutePublicDrugInteractionToolReportRequest request) =>
    new(Request: request);
}
