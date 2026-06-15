using DrugsApp.Features.DrugToolFeature.Shared;
using ErrorOr;

namespace DrugsApp.Features.DrugToolFeature.ApplicationServices;

/// <summary>
/// Internal build result for the drug tool detail response [description].
/// </summary>
public sealed record DrugToolDetailBuildResult(
    Guid DrugId,
    string CommercialName,
    string TypeOfProduct,
    DrugsApp.Domain.DrugFeature.Entities.DrugType TypeOfDrug,
    Guid SelectedDrugFormId,
    DrugToolFormSummaryDto SelectedDrugFormSummary,
    IReadOnlyCollection<DrugToolActiveSubstanceDto> ActiveSubstances,
    DrugToolInteractionScope InteractionScope,
    DrugToolAlcoholSectionDto AlcoholSection,
    DrugToolDrugInteractionSectionDto DrugSection,
    DrugToolFoodInteractionSectionDto FoodSection,
    DrugToolSupplementInteractionSectionDto SupplementSection);

/// <summary>
/// Application service contract for assembling the drug tool detail page [description].
/// </summary>
public interface IDrugToolDetailApplicationService
{
    /// <summary>
    /// Builds the aggregated drug tool detail projection for one drug form.
    /// </summary>
    Task<ErrorOr<DrugToolDetailBuildResult>> BuildAsync(
        Guid drugFormId,
        DrugToolInteractionScope interactionScope,
        Guid userId,
        CancellationToken cancellationToken);
}
