using DrugsApp.Features.DrugInteractionToolFeature.Shared;
using FluentValidation;

namespace DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;

/// <summary>
/// Validates the public drug interaction tool report request shape; the public tool accepts up to 5 items of each kind [description].
/// </summary>
public sealed class ExecutePublicDrugInteractionToolReportRequestValidator
  : AbstractValidator<ExecutePublicDrugInteractionToolReportRequest>
{
  public ExecutePublicDrugInteractionToolReportRequestValidator()
  {
    this.AddIdSelectionRules(request => request.ManualDrugFormIds, "ManualDrugFormIds", 5);
    this.AddIdSelectionRules(request => request.SelectedSupplementIds, "SelectedSupplementIds", 5);
    this.AddIdSelectionRules(request => request.SelectedFoodIds, "SelectedFoodIds", 5);
  }
}
