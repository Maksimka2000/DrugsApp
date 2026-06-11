using FluentValidation;

namespace DrugsApp.Features.DrugInteractionToolFeature.ExecutePublicDrugInteractionToolReport;

/// <summary>
/// Validates the public drug interaction tool report request shape [description].
/// </summary>
public sealed class ExecutePublicDrugInteractionToolReportRequestValidator
  : AbstractValidator<ExecutePublicDrugInteractionToolReportRequest>
{
  public ExecutePublicDrugInteractionToolReportRequestValidator()
  {
    When(
      request => request.ManualDrugFormIds is not null && request.ManualDrugFormIds.Count > 0,
      () =>
      {
        RuleFor(request => request.ManualDrugFormIds!)
          .Must(ids => ids.Count <= 3)
          .WithMessage("ManualDrugFormIds must not contain more than 3 items.")
          .Must(ids => ids.All(id => id != Guid.Empty))
          .WithMessage("ManualDrugFormIds must not contain empty Guid values.")
          .Must(ids => ids.Distinct().Count() == ids.Count)
          .WithMessage("ManualDrugFormIds must be unique.");
      });
  }
}
